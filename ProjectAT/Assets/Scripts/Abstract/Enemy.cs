using UnityEngine;
using System.Collections;
using System;
using Interactable;
using UnityEngine.AI;
using TMPro;
using UnityEngine.Animations.Rigging;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AwarenessModule))]
[RequireComponent(typeof(EnemyAlertnessModule))]
[RequireComponent(typeof(PerceptionSystem))]
public class Enemy : MonoBehaviour, ISquadMember
{
    public event Action<ISquadMember, IPerceivable> onTargetDetected;
    public event Action<ISquadMember, IPerceivable, Vector3> onTargetLost;
    public event Action<ISquadMember, IPerceivable, Vector3> onTargetPositionUpdated;

    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform[] patrolWaypoints;
    [SerializeField] private float positionReportInterval = 0.5f;

    [SerializeField] internal TextMeshProUGUI stateText;
    [SerializeField] private Transform muzzleTransform;

    [SerializeField] private Renderer[] renderersToHide;
    [SerializeField] private Collider[] collidersToDisable;

    [SerializeField] private Transform aimTarget;

    private EntityStatus entityStatus;

    private NavMeshAgent navMeshAgent;
    private PerceptionSystem perceptionSystem;
    private AwarenessModule awarenessModule;
    private EnemyAlertnessModule alertnessModule;
    private FieldOfViewVisuals fieldOfViewVisuals;
    private EnemyAnimator enemyAnimator;
    private Weapon weapon;
    private RigBuilder rigBuilder;

    private Squad squad;
    private IEnemyState currState;

    private IPerceivable currentTarget;
    private Vector3 lastKnownPosition;
    private Vector3 currentOrderDestination;
    private Vector3 investigatePosition;
    private Vector3 investigateReturnPosition;
    private Vector3 investigateReturnDirection;
    private IEnemyState investigateReturnState;
    private Coroutine positionReportCoroutine;
    private WaitForSeconds wait;

    private int wayPointIndex;

    public EnemyData EnemyData => enemyData;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public PerceptionSystem PerceptionSystem => perceptionSystem;
    public AwarenessModule AwarenessModule => awarenessModule;
    public EnemyAlertnessModule AlertnessModule => alertnessModule;
    public float Alertness => alertnessModule != null ? alertnessModule.Alertness : 0f;
    public Vector3 LastKnownStimulusPosition => alertnessModule != null ? alertnessModule.LastKnownStimulusPosition : Vector3.zero;
    public bool HasStimulusPosition => alertnessModule != null && alertnessModule.HasStimulusPosition;
    public Vector3 LastKnownPosition => lastKnownPosition;
    public Vector3 CurrentOrderDestination => currentOrderDestination;
    internal Vector3 InvestigatePosition => investigatePosition;
    internal Vector3 InvestigateReturnPosition => investigateReturnPosition;
    internal Vector3 InvestigateReturnDirection => investigateReturnDirection;
    internal IEnemyState InvestigateReturnState => investigateReturnState;

    // ISquadMember
    public IPerceivable CurrentTarget => currentTarget;
    public bool IsInCombat => ReferenceEquals(currState, IEnemyState.AttackState);
    public bool IsSearching => ReferenceEquals(currState, IEnemyState.SearchState);
    public bool IsChasing => ReferenceEquals(currState, IEnemyState.ChaseState);
    public bool IsEngaging => IsInCombat || IsSearching || IsChasing;

    public bool IsAlive { get; private set; } = true;
    public Transform Transform => transform;

    // State 공유 변수
    internal bool IsAssignedToSquad => squad != null;
    internal bool HasPatrolWaypoints => patrolWaypoints != null && patrolWaypoints.Length > 0;
    internal Transform[] Waypoints => patrolWaypoints;

    internal int WaypointIndex
    {
        get => wayPointIndex;
        set
        {
            if (value < 0 || value >= patrolWaypoints.Length)
                Debug.LogError("value < 0 && value >= patrolWaypoints.Length");

            wayPointIndex = value;
        }
    }

    internal bool UseOrderedDestination { get; set; }
    internal bool PatrolPausedByTarget { get; set; }
    internal float Elapsed { get; set; }
    internal bool ArrivedOnce { get; set; }

    internal EnemySearchState.Phase Phase { get; set; }
    internal Vector3 SearchCenter;
    internal Vector3 CurrentPoint;
    internal float WaitTimer;
    internal EnemyInvestigateState.Phase InvestigatePhase { get; set; }

    internal EnemyAnimator EnemyAnimator => enemyAnimator;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        awarenessModule = GetComponent<AwarenessModule>();
        alertnessModule = GetComponent<EnemyAlertnessModule>();
        fieldOfViewVisuals = GetComponent<FieldOfViewVisuals>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        weapon = GetComponentInChildren<Gun>();
        entityStatus = GetComponent<EntityStatus>();
        rigBuilder = GetComponent<RigBuilder>();

        renderersToHide = GetComponentsInChildren<Renderer>(true);
        collidersToDisable = GetComponentsInChildren<Collider>(true);

        perceptionSystem.Initialize(enemyData.ViewAngle, enemyData.SearchRadius, enemyData.SecondaryViewRadius);

        wait = new WaitForSeconds(positionReportInterval);

        AimAtTarget(false);
    }

    private void OnEnable()
    {
        perceptionSystem.onTargetDetected += TargetDetected;
        awarenessModule.onScanStarted += ScanStarted;
        awarenessModule.onTargetConfirmed += TargetConfirmed;
        awarenessModule.onTargetLost += TargetLost;
        entityStatus.onDeath += Die;
        perceptionSystem.onCorpseDetected += CorpseDetected;

        if (alertnessModule is not null)
            alertnessModule.onAlertThresholdReached += AlertThresholdReached;
    }

    private void OnDisable()
    {
        perceptionSystem.onTargetDetected -= TargetDetected;
        awarenessModule.onScanStarted -= ScanStarted;
        awarenessModule.onTargetConfirmed -= TargetConfirmed;
        awarenessModule.onTargetLost -= TargetLost;
        entityStatus.onDeath -= Die;
        perceptionSystem.onCorpseDetected -= CorpseDetected;

        if (alertnessModule is not null)
            alertnessModule.onAlertThresholdReached -= AlertThresholdReached;

        StopPositionReport();
    }

    private void Start()
    {
        ChangeState(!IsAssignedToSquad && HasPatrolWaypoints ? IEnemyState.PatrolState : IEnemyState.IdleState);
    }

    private void Update()
    {
        currState.Update(this);

        enemyAnimator.SetSpeed(navMeshAgent.velocity.magnitude);
    }

    private void TargetConfirmed(IPerceivable target)
    {
        ConfirmTarget(target);
    }

    private void ConfirmTarget(IPerceivable target)
    {
        if (target is null || !target.IsValidTarget) return;

        currentTarget = target;
        lastKnownPosition = target.Transform.position;
        currentOrderDestination = lastKnownPosition;

        currState?.TargetConfirmed(this, target);

        StartPositionReport();

        onTargetDetected?.Invoke(this, target);
    }

    private void TargetDetected(IPerceivable target)
    {
        currState?.TargetDetected(this, target);
    }

    private void ScanStarted(IPerceivable target)
    {
        if (target is null || target.Transform is null) return;

        ReportStimulus(target.Transform.position, StimulusType.BriefSight);
    }

    private void TargetLost(IPerceivable target)
    {
        if (!ReferenceEquals(currentTarget, target)) return;

        Vector3 lastPosition = target.IsValidTarget ? target.Transform.position : lastKnownPosition;
        lastKnownPosition = lastPosition;
        currentTarget = null;

        currState?.TargetLost(this, target);

        StopPositionReport();

        onTargetLost?.Invoke(this, target, lastPosition);
    }

    private void CorpseDetected(DownedBody downedBody)
    {
        if (!IsAlive || downedBody is null) return;
        if (currentTarget is not null && currentTarget.IsValidTarget) return;

        if (squad is not null)
        {
            squad.TryReportCorpseFound(this, downedBody);
            return;
        }

        Vector3 corpsePosition = downedBody.Transform.position;
        currentOrderDestination = corpsePosition;
        lastKnownPosition = corpsePosition;

        ReceiveOrder(SquadOrder.Search(corpsePosition));
    }

    private void Die()
    {
        if (!IsAlive) return;
        IsAlive = false;

        ChangeState(IEnemyState.DeadState);
    }

    public void ReceiveOrder(SquadOrder squadOrder)
    {
        currentOrderDestination = squadOrder.Position;
        UseOrderedDestination = squadOrder.OrderKind == OrderKind.Patrol;

        if (currState is null)
        {
            ChangeState(GetStateForOrder(squadOrder));
            return;
        }

        currState?.OrderReceived(this, squadOrder);
    }

    public void ReportStimulus(Vector3 position, StimulusType stimulusType)
    {
        alertnessModule.ReportStimulus(position, stimulusType);
    }

    public void ReportStimulus(Vector3 position, float amount)
    {
        alertnessModule.ReportStimulus(position, amount);
    }

    public void NoiseDetected(Vector3 noisePosition)
    {
        if (!IsAlive) return;

        ReportStimulus(noisePosition, StimulusType.Sound);

        currState?.NoiseDetected(this, noisePosition);
    }

    public void ReceiveAttack(IPerceivable attacker)
    {
        if (attacker is null || !attacker.IsValidTarget) return;

        ConfirmTarget(attacker);
    }

    private static IEnemyState GetStateForOrder(SquadOrder squadOrder)
    {
        return squadOrder.OrderKind switch
        {
            OrderKind.Attack => IEnemyState.ChaseState,
            OrderKind.Search => IEnemyState.SearchState,
            OrderKind.Patrol => IEnemyState.PatrolState,
            OrderKind.Disengage => IEnemyState.IdleState,
            _ => IEnemyState.IdleState,
        };
    }

    internal void JoinSquad(Squad squad)
    {
        this.squad = squad;
    }

    internal void LeaveSquad(Squad squad)
    {
        if (ReferenceEquals(this.squad, squad))
            this.squad = null;
    }

    private void AlertThresholdReached(Vector3 position, float alertness)
    {
        lastKnownPosition = position;

        if (squad is null) return;

        squad.ReportMemberAlert(this, position, alertness);
    }

    internal void ChangeState(IEnemyState nextState)
    {
        if (ReferenceEquals(currState, nextState)) return;

        currState?.Exit(this);
        currState = nextState;
        currState.Enter(this);
    }

    internal void MoveTo(Vector3 position)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(position);
    }

    internal void StopMoving()
    {
        // Debug.Log($"[StopMoving] enabled={navMeshAgent.enabled}, " +
        //               $"activeAndEnabled={navMeshAgent.isActiveAndEnabled}, " +
        //               $"isOnNavMesh={navMeshAgent.isOnNavMesh}, " +
        //               $"pos={transform.position}");

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    internal bool HasArrived()
    {
        if (navMeshAgent.pathPending) return false;

        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f;
    }

    internal bool IsTargetInAttackRange()
    {
        // CombatModule 로 모듈화 할 것

        if (currentTarget is null || !currentTarget.IsValidTarget) return false;

        float distance = (currentTarget.Transform.position - transform.position).sqrMagnitude;
        return distance <= enemyData.AttackRange * enemyData.AttackRange;
    }

    internal bool RotateTowardTarget()
    {
        if (currentTarget is null || !currentTarget.IsValidTarget) return false;

        Vector3 targetPos = CurrentTarget.Transform.position;
        Vector3 direction = targetPos - muzzleTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < float.Epsilon) return true;

        Vector3 currentMuzzleDir = muzzleTransform.forward;
        currentMuzzleDir.y = 0f;

        Quaternion delta = Quaternion.FromToRotation(currentMuzzleDir, direction);
        Quaternion targetBodyRotation = delta * transform.rotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetBodyRotation, enemyData.RotateSpeed * Time.deltaTime);

        float angle = Vector3.Angle(muzzleTransform.forward, direction);
        return angle <= enemyData.AimAngleThreshold;
    }

    internal void Fire()
    {
        // Debug.Log("Fire");
        weapon.Attack();
    }

    internal void AimAtTarget(bool isActive)
    {
        if (currentTarget is not null)
            aimTarget.transform.position = currentTarget.Transform.position + Vector3.up;

        foreach (RigLayer rigLayer in rigBuilder.layers)
            rigLayer.active = isActive;
    }

    internal void EnableFieldOfView(bool isEnable)
    {
        fieldOfViewVisuals.enabled = isEnable;
    }

    internal void SetAttackMode(bool isAttackMode)
    {
        perceptionSystem.SetAttackMode(isAttackMode);
    }

    internal void SetInvestigateContext(Vector3 noisePosition)
    {
        investigatePosition = noisePosition;
        investigateReturnPosition = transform.position;
        investigateReturnDirection = transform.forward;
        investigateReturnState = currState;
    }

    internal void HideOriginalVisual()
    {
        foreach (Renderer renderer in renderersToHide)
            renderer.enabled = false;

        foreach (Collider collider in collidersToDisable)
            collider.enabled = false;
    }

    private void StartPositionReport()
    {
        if (positionReportCoroutine is null)
            positionReportCoroutine = StartCoroutine(PositionReportCoroutine());
    }

    internal void StopPositionReport()
    {
        if (positionReportCoroutine is not null)
        {
            StopCoroutine(positionReportCoroutine);
            positionReportCoroutine = null;
        }
    }

    private IEnumerator PositionReportCoroutine()
    {
        while (currentTarget is not null && currentTarget.IsValidTarget)
        {
            lastKnownPosition = currentTarget.Transform.position;

            Debug.Log("onTargetPositionUpdated");
            onTargetPositionUpdated?.Invoke(this, currentTarget, lastKnownPosition);

            yield return wait;
        }

        positionReportCoroutine = null;
    }

    internal void SpawnCorpse()
    {
        if (enemyData.CorpsePrefab is null) return;

        Instantiate(enemyData.CorpsePrefab, transform.position, transform.rotation);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastKnownPosition, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentOrderDestination, 1f);
    }
#endif
}
