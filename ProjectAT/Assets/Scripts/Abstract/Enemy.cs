using UnityEngine;
using System.Collections;
using System;
using Interactable;
using UnityEngine.AI;
using TMPro;
using UnityEngine.Animations.Rigging;
using ProjectAT.FieldUI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AwarenessModule))]
[RequireComponent(typeof(EnemyAlertnessModule))]
[RequireComponent(typeof(PerceptionSystem))]
[RequireComponent(typeof(CrowdControlModule))]
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
    private CrowdControlModule crowdControlModule;
    private RigBuilder rigBuilder;
    private CoverHandler coverHandler;
    private StatusViewController statusViewController;

    private Squad squad;
    private IEnemyState currState;
    private IEnemyState stateBeforeStun;

    private IPerceivable currentTarget;
    private CoverPoint reservedCoverPoint;
    private Vector3 lastKnownPosition;
    private Vector3 currentOrderDestination;
    private Vector3 reservedCoverDestination;
    private Vector3 investigatePosition;
    private Vector3 investigateReturnPosition;
    private Vector3 investigateReturnDirection;
    private IEnemyState investigateReturnState;
    private Coroutine positionReportCoroutine;
    private WaitForSeconds wait;
    private NavMeshPath coverPath;
    private readonly Collider[] coverColliders = new Collider[16];

    private int wayPointIndex;
    private float nextCoverSearchTime;
    private float coverTimer;
    private float coverHideDuration;
    private bool isInCover;

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
    public bool IsInCombat => ReferenceEquals(currState, IEnemyState.AttackState) || ReferenceEquals(currState, IEnemyState.CoverState);
    public bool IsSearching => ReferenceEquals(currState, IEnemyState.SearchState);
    public bool IsChasing => ReferenceEquals(currState, IEnemyState.ChaseState);
    public bool IsEngaging => IsInCombat || IsSearching || IsChasing;

    public bool IsAlive { get; private set; } = true;
    public Transform Transform => transform;
    internal bool HasValidCurrentTarget => currentTarget is not null && currentTarget.IsValidTarget;
    internal bool HasReservedCover => reservedCoverPoint is not null;
    internal bool IsMovingToCover => reservedCoverPoint is not null && !isInCover;
    internal bool IsInCover => reservedCoverPoint is not null && isInCover;
    internal bool IsStunned => crowdControlModule != null && crowdControlModule.IsStunned;
    internal ICoverSubState CoverSubState { get; set; }

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
        crowdControlModule = GetComponent<CrowdControlModule>();
        if (crowdControlModule == null)
        {
            crowdControlModule = gameObject.AddComponent<CrowdControlModule>();
        }

        if (statusViewController == null)
        {
            statusViewController = GetComponentInChildren<StatusViewController>();
            if(statusViewController == null)
            {
                GameObject statusViewControllerGO = new GameObject("OverlayUIController");
                statusViewControllerGO.transform.SetParent(transform);
                statusViewController = statusViewControllerGO.AddComponent<StatusViewController>();
            }
        }

        entityStatus = GetComponent<EntityStatus>();
        rigBuilder = GetComponent<RigBuilder>();
        coverHandler = GetComponent<CoverHandler>();

        renderersToHide = GetComponentsInChildren<Renderer>(true);
        collidersToDisable = GetComponentsInChildren<Collider>(true);

        perceptionSystem.Initialize(enemyData.ViewAngle, enemyData.SearchRadius, enemyData.SecondaryViewRadius);

        wait = new WaitForSeconds(positionReportInterval);
        coverPath = new NavMeshPath();

        AimAtTarget(false);
    }

    private void OnEnable()
    {
        perceptionSystem.onTargetDetected += TargetDetected;
        awarenessModule.onScanStarted += ScanStarted;
        awarenessModule.onTargetConfirmed += TargetConfirmed;
        awarenessModule.onTargetLost += TargetLost;
        entityStatus.onDeath += Die;
        crowdControlModule.OnStunStarted += HandleStunStarted;
        crowdControlModule.OnStunEnded += HandleStunEnded;
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
        if (crowdControlModule != null)
        {
            crowdControlModule.OnStunStarted -= HandleStunStarted;
            crowdControlModule.OnStunEnded -= HandleStunEnded;
        }

        perceptionSystem.onCorpseDetected -= CorpseDetected;

        if (alertnessModule is not null)
            alertnessModule.onAlertThresholdReached -= AlertThresholdReached;

        ReleaseCover();
        StopPositionReport();
    }

    private void Start()
    {
        ChangeState(IsStunned ? IEnemyState.StunnedState : !IsAssignedToSquad && HasPatrolWaypoints ? IEnemyState.PatrolState : IEnemyState.IdleState);
    }

    private void Update()
    {
        currState?.Update(this);

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

        if (ReferenceEquals(currState, IEnemyState.CoverState))
        {
            return;
        }

        currentTarget = null;

        currState?.TargetLost(this, target);

        StopPositionReport();

        onTargetLost?.Invoke(this, target, lastPosition);
    }

    private void CorpseDetected(DownedBody downedBody)
    {
        if (!IsAlive || downedBody is null) return;
        if (HasValidCurrentTarget) return;

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
        stateBeforeStun = null;

        ChangeState(IEnemyState.DeadState);
    }

    public void ReceiveOrder(SquadOrder squadOrder)
    {
        currentOrderDestination = squadOrder.Position;
        UseOrderedDestination = squadOrder.OrderKind == OrderKind.Patrol;

        if (squadOrder.OrderKind == OrderKind.Attack && squadOrder.Target is not null && squadOrder.Target.IsValidTarget)
        {
            currentTarget = squadOrder.Target;
            lastKnownPosition = squadOrder.Target.Transform.position;
        }

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
        if (nextState is null) return;
        if (IsStunned && !ReferenceEquals(nextState, IEnemyState.StunnedState) && !ReferenceEquals(nextState, IEnemyState.DeadState))
        {
            return;
        }

        if (ReferenceEquals(currState, nextState)) return;

        currState?.Exit(this);
        currState = nextState;
        currState.Enter(this);
    }

    internal void MoveTo(Vector3 position)
    {
        if (IsStunned) return;

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

    internal void MoveTowardCurrentTarget()
    {
        if (IsStunned) return;
        if (!HasValidCurrentTarget) return;

        Vector3 targetPosition = currentTarget.Transform.position;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, enemyData.SearchNavSampleRadius, NavMesh.AllAreas))
            MoveTo(hit.position);
        else
            MoveTo(targetPosition);
    }

    internal bool TryReserveBestCover()
    {
        if (!enemyData.UseCover) return false;
        if (reservedCoverPoint is not null) return true;
        if (!HasValidCurrentTarget) return false;
        if (Time.time < nextCoverSearchTime) return false;

        nextCoverSearchTime = Time.time + enemyData.CoverSearchCooldown;

        if (!TryFindBestCoverPoint(out CoverPoint coverPoint, out Vector3 destination)) return false;
        if (!coverPoint.Reserve(gameObject)) return false;

        reservedCoverPoint = coverPoint;
        reservedCoverDestination = destination;
        isInCover = false;

        if (coverHandler is not null)
            coverHandler.currentCover = null;

        return true;
    }

    internal bool MoveToReservedCover()
    {
        if (reservedCoverPoint is null) return false;

        if (reservedCoverPoint.CurrentInteractor != gameObject)
        {
            ReleaseCover();
            return false;
        }

        if (coverHandler is not null)
            coverHandler.currentCover = null;

        SetCoverCrouch(false);
        MoveTo(reservedCoverDestination);
        return true;
    }

    internal bool TryEnterCoverIfArrived()
    {
        if (reservedCoverPoint is null || isInCover) return false;

        if (reservedCoverPoint.CurrentInteractor != gameObject)
        {
            ReleaseCover();
            return false;
        }

        if (!HasArrived()) return false;

        StopMoving();
        isInCover = true;
        SetCoverCrouch(true);

        if (coverHandler is not null)
            coverHandler.currentCover = reservedCoverPoint.transform;

        return true;
    }

    internal void ReleaseCover()
    {
        if (reservedCoverPoint is not null && reservedCoverPoint.CurrentInteractor == gameObject)
            reservedCoverPoint.Release();

        reservedCoverPoint = null;
        reservedCoverDestination = Vector3.zero;
        isInCover = false;

        if (coverHandler is not null)
            coverHandler.currentCover = null;

        SetCoverCrouch(false);
    }

    internal void SetCoverCrouch(bool isCrouch)
    {
        if (enemyAnimator is not null)
            enemyAnimator.SetCrouch(isCrouch);
    }

    internal void ResetCoverHideTimer()
    {
        coverTimer = 0f;
        float min = Mathf.Min(enemyData.MinHideTime, enemyData.MaxHideTime);
        float max = Mathf.Max(enemyData.MinHideTime, enemyData.MaxHideTime);
        coverHideDuration = UnityEngine.Random.Range(min, max);
    }

    internal bool UpdateCoverHideTimer(float deltaTime)
    {
        coverTimer += deltaTime;
        return coverTimer >= coverHideDuration;
    }

    internal bool IsWeaponReloading()
    {
        return weapon is not null && weapon.IsReloading;
    }

    internal bool HasArrived()
    {
        if (navMeshAgent.pathPending) return false;

        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.1f;
    }

    internal bool IsTargetInAttackRange()
    {
        // CombatModule 로 모듈화 할 것

        if (!HasValidCurrentTarget) return false;

        float distance = (currentTarget.Transform.position - transform.position).sqrMagnitude;
        return distance <= enemyData.AttackRange * enemyData.AttackRange;
    }

    private bool TryFindBestCoverPoint(out CoverPoint bestCoverPoint, out Vector3 bestDestination)
    {
        bestCoverPoint = null;
        bestDestination = default;

        int coverMask = LayerMask.GetMask("CoverPoint");
        if (coverMask == 0) return false;

        Vector3 targetPosition = currentTarget.Transform.position;
        float attackRangeSqr = enemyData.AttackRange * enemyData.AttackRange;
        int count = Physics.OverlapSphereNonAlloc(transform.position, enemyData.CoverSearchRadius, coverColliders, coverMask, QueryTriggerInteraction.Collide);

        float bestScore = float.MaxValue;

        for (int i = 0; i < count; ++i)
        {
            CoverPoint coverPoint = coverColliders[i].GetComponentInParent<CoverPoint>();
            if (coverPoint is null) continue;
            if (coverPoint.CurrentInteractor is not null && coverPoint.CurrentInteractor != gameObject) continue;
            if ((coverPoint.transform.position - targetPosition).sqrMagnitude > attackRangeSqr) continue;

            bool blocksTarget = HasCoverObstacleBetween(coverPoint, targetPosition);
            if (!blocksTarget && !IsOnOppositeSideOfCover(coverPoint, targetPosition)) continue;
            if (!TryGetCoverDestination(coverPoint, out Vector3 destination)) continue;
            if (!HasCompletePathTo(destination)) continue;

            float score = (destination - transform.position).sqrMagnitude;
            if (!blocksTarget)
                score += enemyData.CoverSearchRadius * enemyData.CoverSearchRadius;

            if (score >= bestScore) continue;

            bestScore = score;
            bestCoverPoint = coverPoint;
            bestDestination = destination;
        }

        return bestCoverPoint is not null;
    }

    private bool TryGetCoverDestination(CoverPoint coverPoint, out Vector3 destination)
    {
        if (NavMesh.SamplePosition(coverPoint.transform.position, out NavMeshHit hit, enemyData.CoverNavSampleRadius, NavMesh.AllAreas))
        {
            destination = hit.position;
            return true;
        }

        destination = default;
        return false;
    }

    private bool HasCompletePathTo(Vector3 destination)
    {
        return navMeshAgent.isOnNavMesh
            && navMeshAgent.CalculatePath(destination, coverPath)
            && coverPath.status == NavMeshPathStatus.PathComplete;
    }

    private bool HasCoverObstacleBetween(CoverPoint coverPoint, Vector3 targetPosition)
    {
        if (perceptionSystem.ObstacleMask.value == 0) return false;

        Vector3 from = targetPosition + Vector3.up * 0.5f;
        Vector3 to = coverPoint.transform.position + Vector3.up * 0.5f;
        Vector3 delta = to - from;
        float distance = delta.magnitude;

        if (distance < float.Epsilon) return false;

        return Physics.Raycast(from, delta / distance, distance, perceptionSystem.ObstacleMask, QueryTriggerInteraction.Ignore);
    }

    private bool IsOnOppositeSideOfCover(CoverPoint coverPoint, Vector3 targetPosition)
    {
        CoverObject coverObject = coverPoint.GetComponentInParent<CoverObject>();
        if (coverObject is null) return true;

        Vector3 coverToPoint = coverPoint.transform.position - coverObject.transform.position;
        Vector3 coverToTarget = targetPosition - coverObject.transform.position;
        coverToPoint.y = 0f;
        coverToTarget.y = 0f;

        if (coverToPoint.sqrMagnitude < float.Epsilon || coverToTarget.sqrMagnitude < float.Epsilon)
            return false;

        float angle = Vector3.Angle(coverToPoint, coverToTarget);
        return angle >= enemyData.CoverOppositeSideAngleThreshold;
    }

    internal bool RotateTowardTarget()
    {
        if (IsStunned) return false;
        if (!HasValidCurrentTarget) return false;

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
        if (IsStunned) return;

        // Debug.Log("Fire");
        weapon.Attack();
    }

    internal void AimAtTarget(bool isActive)
    {
        if (IsStunned && isActive) return;

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
        if (IsStunned && isAttackMode) return;

        perceptionSystem.SetAttackMode(isAttackMode);
    }

    private void HandleStunStarted()
    {
        if (!IsAlive) return;

        if (!ReferenceEquals(currState, IEnemyState.StunnedState) && !ReferenceEquals(currState, IEnemyState.DeadState))
        {
            stateBeforeStun = currState;
        }

        ChangeState(IEnemyState.StunnedState);
    }

    private void HandleStunEnded()
    {
        if (!IsAlive) return;

        if (ReferenceEquals(currState, IEnemyState.StunnedState))
        {
            ChangeState(stateBeforeStun ?? IEnemyState.IdleState);
        }

        stateBeforeStun = null;
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
        while (HasValidCurrentTarget)
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
