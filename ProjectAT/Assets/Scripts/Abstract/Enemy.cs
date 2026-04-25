using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AwarenessModule))]
[RequireComponent(typeof(PerceptionSystem))]
public class Enemy : MonoBehaviour, ISquadMember
{
    public event Action<ISquadMember, IPerceivable> onTargetDetected;
    public event Action<ISquadMember, IPerceivable, Vector3> onTargetLost;
    public event Action<ISquadMember, IPerceivable, Vector3> onTargetPositionUpdated;

    [SerializeField] private EnemyData enemyData;
    [SerializeField] private float positionReportInterval = 0.5f;

    private NavMeshAgent navMeshAgent;
    private PerceptionSystem perceptionSystem;
    private AwarenessModule awarenessModule;

    private IEnemyState currState;
    private IPerceivable currentTarget;
    private Vector3 lastKnownPosition;
    private Vector3 currentOrderDestination;
    private Coroutine positionReportCoroutine;

    public EnemyData EnemyData => enemyData;
    public NavMeshAgent NavMeshAgent => navMeshAgent;
    public PerceptionSystem PerceptionSystem => perceptionSystem;
    public AwarenessModule AwarenessModule => awarenessModule;
    public Vector3 LastKnownPosition => lastKnownPosition;
    public Vector3 CurrentOrderDestination => currentOrderDestination;

    // ISquadMember
    public IPerceivable CurrentTarget => currentTarget;
    public bool IsEngaging => currState == IEnemyState.AttackState;
    public bool IsAlive { get; private set; } = true;
    public Transform Transform => transform;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        awarenessModule = GetComponent<AwarenessModule>();

        perceptionSystem.Initialize(enemyData.ViewAngle, enemyData.PrimaryViewRadius, enemyData.SecondaryViewRadius);
    }

    private void OnEnable()
    {
        awarenessModule.onTargetConfirmed += TargetConfirmed;
        awarenessModule.onTargetLost += TargetLost;
    }

    private void OnDisable()
    {
        awarenessModule.onTargetConfirmed -= TargetConfirmed;
        awarenessModule.onTargetLost -= TargetLost;

        StopPositionReport();
    }

    private void Start()
    {
        ChangeState(IEnemyState.IdleState);
    }

    private void Update()
    {
        currState.Update(this);
    }

    private void TargetConfirmed(IPerceivable target)
    {
        bool isFirstAcquisition = currentTarget is null;

        currentTarget = target;
        lastKnownPosition = target.Transform.position;

        onTargetDetected?.Invoke(this, target);

        if (isFirstAcquisition)
            ChangeState(IEnemyState.AttackState);

        StartPositionReport();
    }

    private void TargetLost(IPerceivable target)
    {
        if (!ReferenceEquals(currentTarget, target)) return;

        Vector3 lastPosition = target.IsValidTarget ? target.Transform.position : lastKnownPosition;
        lastKnownPosition = lastPosition;
        currentTarget = null;

        onTargetLost?.Invoke(this, target, lastPosition);

        StopPositionReport();

        if (currState == IEnemyState.AttackState)
            ChangeState(IEnemyState.SearchState);
    }

    public void ReceiveOrder(SquadOrder squadOrder)
    {
        switch (squadOrder.OrderKind)
        {
            case OrderKind.Attack:
                HandleAttackOrder(squadOrder);
                break;

            case OrderKind.Search:
                HandleSearchOrder(squadOrder);
                break;

            case OrderKind.Patrol:
                HandlePatrolOrder(squadOrder);
                break;

            case OrderKind.Disengage:
                if (currState == IEnemyState.AttackState) ChangeState(IEnemyState.IdleState);
                break;
        }
    }

    private void HandleAttackOrder(SquadOrder order)
    {
        currentOrderDestination = order.Position;

        // ★ 자율성 규칙 ★
        // 이미 교전 중이고, 그 타겟이 유효하면 다른 타겟으로 바꾸지 않는다.
        if (currState == IEnemyState.AttackState && currentTarget is not null && currentTarget.IsValidTarget)
        {
            if (!ReferenceEquals(currentTarget, order.Target))
            {
                // 명령된 타겟은 무시, 현재 타겟 계속 공격
                return;
            }

            // 같은 타겟에 대한 갱신이면 슬롯만 갱신 (Tick에서 사용)
            lastKnownPosition = order.Target.Transform.position;
            return;
        }

        // 교전 중이 아니거나, 타겟이 유효하지 않으면 명령 수용
        currentTarget = order.Target;
        lastKnownPosition = order.Target.Transform.position;
        ChangeState(IEnemyState.AttackState);
    }

    private void HandleSearchOrder(SquadOrder order)
    {
        // 교전 중이면 Search 명령 무시 (자기 타겟 유지)
        if (currState == IEnemyState.AttackState) return;

        currentOrderDestination = order.Position;
        lastKnownPosition = order.Position;
        ChangeState(IEnemyState.SearchState);
    }

    private void HandlePatrolOrder(SquadOrder order)
    {
        // 교전 중이면 Patrol 명령 무시
        if (currState == IEnemyState.AttackState) return;

        currentOrderDestination = order.Position;
        ChangeState(IEnemyState.PatrolState);
    }

    internal void ChangeState(IEnemyState nextState)
    {
        if (ReferenceEquals(currState, nextState)) return;

        currState?.Exit(this);
        currState = nextState;
        perceptionSystem.SetAttackMode(currState == IEnemyState.AttackState);
        currState.Enter(this);
    }

    internal void MoveTo(Vector3 position)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(position);
    }

    internal void StopMoving()
    {
        Debug.Log($"[StopMoving] enabled={navMeshAgent.enabled}, " +
                      $"activeAndEnabled={navMeshAgent.isActiveAndEnabled}, " +
                      $"isOnNavMesh={navMeshAgent.isOnNavMesh}, " +
                      $"pos={transform.position}");

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
        if (currentTarget is null || !currentTarget.IsValidTarget) return false;

        float distance = (currentTarget.Transform.position - transform.position).sqrMagnitude;
        return distance <= enemyData.AttackRange * enemyData.AttackRange;
    }

    internal void Fire()
    {

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
        WaitForSeconds wait = new WaitForSeconds(positionReportInterval);

        while (currentTarget is not null && currentTarget.IsValidTarget)
        {
            lastKnownPosition = currentTarget.Transform.position;
            onTargetPositionUpdated?.Invoke(this, currentTarget, lastKnownPosition);
            Debug.Log("onTargetPositionUpdated");

            yield return wait;
        }

        positionReportCoroutine = null;
    }
}
