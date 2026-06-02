using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;
using TMPro;

// CombatMoudle로 한번 모듈화 하고..!

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AwarenessModule))]
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

    private NavMeshAgent navMeshAgent;
    private PerceptionSystem perceptionSystem;
    private AwarenessModule awarenessModule;
    private FieldOfViewVisuals fieldOfViewVisuals;
    private EnemyAnimator enemyAnimator;
    private Weapon weapon;

    private Squad squad;
    private IEnemyState currState;

    private IPerceivable currentTarget;
    private Vector3 lastKnownPosition;
    private Vector3 currentOrderDestination;
    private Coroutine positionReportCoroutine;
    private WaitForSeconds wait;

    private int wayPointIndex;

    public EnemyData EnemyData => enemyData;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public PerceptionSystem PerceptionSystem => perceptionSystem;
    public AwarenessModule AwarenessModule => awarenessModule;
    public Vector3 LastKnownPosition => lastKnownPosition;
    public Vector3 CurrentOrderDestination => currentOrderDestination;

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

    internal EnemyAnimator EnemyAnimator => enemyAnimator;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        awarenessModule = GetComponent<AwarenessModule>();
        fieldOfViewVisuals = GetComponent<FieldOfViewVisuals>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        weapon = GetComponentInChildren<Gun>();

        perceptionSystem.Initialize(enemyData.ViewAngle, enemyData.SearchRadius, enemyData.SecondaryViewRadius);

        wait = new WaitForSeconds(positionReportInterval);
    }

    private void OnEnable()
    {
        perceptionSystem.onTargetDetected += TargetDetected;
        awarenessModule.onTargetConfirmed += TargetConfirmed;
        awarenessModule.onTargetLost += TargetLost;
    }

    private void OnDisable()
    {
        perceptionSystem.onTargetDetected -= TargetDetected;
        awarenessModule.onTargetConfirmed -= TargetConfirmed;
        awarenessModule.onTargetLost -= TargetLost;

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
        currentTarget = target;
        lastKnownPosition = target.Transform.position;

        currState?.TargetConfirmed(this, target);

        StartPositionReport();

        onTargetDetected?.Invoke(this, target);
    }

    private void TargetDetected(IPerceivable target)
    {
        currState?.TargetDetected(this, target);
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

    public void ReceiveOrder(SquadOrder squadOrder)
    {
        currentOrderDestination = squadOrder.Position;
        UseOrderedDestination = squadOrder.OrderKind == OrderKind.Patrol;

        if (currState == null)
        {
            ChangeState(GetStateForOrder(squadOrder));
            return;
        }

        currState?.OrderReceived(this, squadOrder);
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

    internal void EnableFieldOfView(bool isEnable)
    {
        fieldOfViewVisuals.enabled = isEnable;
    }

    internal void SetAttackMode(bool isAttackMode)
    {
        perceptionSystem.SetAttackMode(isAttackMode);
    }

    private void StartPositionReport()
    {
        if (positionReportCoroutine is null)
            positionReportCoroutine = StartCoroutine(PositionReportCoroutine());
    }

    private void StopPositionReport()
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
