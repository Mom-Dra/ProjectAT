using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
using MomDra.Weapon;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(FieldOfViewVisuals))]
[RequireComponent(typeof(TargetDetector))]
[RequireComponent(typeof(EnemyAnimator))]
public abstract class Enemy : MonoBehaviour, IAttackable, ISquadMember
{
    public event Action<ISquadMember, IPerceivable, Vector3> onPlayerDetected;
    public event Action<ISquadMember, Vector3> onPlayerLosted;
    public event Action<ISquadMember, Vector3> onPlayerPositionUpdated;

    internal int CurrentWaypointIndex;

    internal ICoverSubState CurrCoverSubState;

    protected NavMeshAgent navMeshAgent;
    protected FieldOfViewVisuals fieldOfViewVisual;
    protected TargetDetector targetDetector;
    protected EnemyAnimator enemyAnimator;
    protected IEnemyState currentState;

    // Inspector���� ����
    [SerializeField]
    private EnemyData enemyData;
    [SerializeField]
    private AlertData alertData;
    [SerializeField]
    private bool isPatrolEnemy;
    [SerializeField]
    private Transform[] patrolWaypoints;
    [SerializeField]
    private float targetCheckInterval = 0.2f;
    [SerializeField]
    private int alertLevel;

    // Debug�� ����
    [SerializeField]
    private Enemy_State enemyState;

    [SerializeField]
    private WeaponType weaponType;

    private float timeSinceTargetLost;
    private float hideTimer;
    private float hideDuration;
    private float peekTimer;
    private Vector3 targetLastKnownPosition;
    private Vector3 targetDestination;
    private Weapon weapon;
    private Transform muzzle;
    private EntityStatus entityStatus;

    private CoverPoint reservedCoverPoint;

    private WaitForSeconds targetCheckWait;
    private WaitForSeconds informPlayerPositionWait;
    private Coroutine targetCheckCoroutine;
    private Coroutine informPlayerPositionCoroutine;

    private TextMeshProUGUI stateText;

    public bool IsPlayerStillVisible => enemyState == Enemy_State.Chase || enemyState == Enemy_State.Attack;

    public EnemyData EnemyData => enemyData;
    public AlertData AlertData => alertData;
    public int AlertLevel => alertLevel;
    internal NavMeshAgent NavMeshAgent => navMeshAgent;
    internal TargetDetector TargetDetector => targetDetector;

    internal IReadOnlyList<Transform> PatrolWaypoints => patrolWaypoints;
    internal Vector3 TargetLastKnownPosition => targetLastKnownPosition;

    internal CoverPoint ReservedCoverPoint => reservedCoverPoint;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        fieldOfViewVisual = GetComponent<FieldOfViewVisuals>();
        targetDetector = GetComponent<TargetDetector>();
        stateText = GetComponentInChildren<TextMeshProUGUI>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        entityStatus = GetComponent<EntityStatus>();

        fieldOfViewVisual.SetEnemyData(this);
        targetDetector.SetEnemyData(this);

        fieldOfViewVisual.onScanComplete += ScanCompleted;
        fieldOfViewVisual.onScanCancel += ScanCanceled;
        fieldOfViewVisual.onScanStart += ScanStarted;

        targetDetector.onTargetDetect += TargetDetected;
        targetDetector.onTargetLosted += TargetLosted;

        muzzle = transform.FindChildRecursive("FX_Shoot_01_muzzle");
        weapon = GetComponentInChildren<Weapon>();

        targetCheckWait = new WaitForSeconds(targetCheckInterval);
        informPlayerPositionWait = new WaitForSeconds(enemyData.TargetInformInterval);

        if (isPatrolEnemy) ChangeState(IEnemyState.PatrolState);
        else ChangeState(IEnemyState.IdleState);
    }

    private void OnDestroy()
    {
        fieldOfViewVisual.onScanComplete -= ScanCompleted;
        fieldOfViewVisual.onScanCancel -= ScanCanceled;
        fieldOfViewVisual.onScanStart -= ScanStarted;


        targetDetector.onTargetDetect -= TargetDetected;
        targetDetector.onTargetLosted -= TargetLosted;
    }

    private void OnEnable()
    {
        entityStatus.onDeath += EnemyDied;
    }

    private void OnDisable()
    {
        entityStatus.onDeath -= EnemyDied;
    }

    private void Start()
    {
        enemyAnimator.SetWeaponType(weaponType);
    }

    private void Update()
    {
        currentState.Update(this);

        enemyAnimator?.SetSpeed(navMeshAgent.velocity.magnitude);
    }

    internal void EnableFieldOfView(bool enabled)
    {
        if (enabled)
            ColorDebug.OrangeLog($"EnableFieldOfView: {enabled}");
        fieldOfViewVisual.enabled = enabled;
    }

    internal void SetAttackMode(bool isAttackMode)
    {
        EnableFieldOfView(!isAttackMode);
        targetDetector.SetAttackMode(isAttackMode);
    }

    public void Attack()
    {
        weapon.Attack();
    }

    internal bool IsReloading()
    {
        return weapon.IsReloading;
    }

    internal void Chase()
    {
        if (navMeshAgent.destination != targetDestination)
        {
            ColorDebug.RedLog($"Chase, targetDeestination: {targetDestination}");
            navMeshAgent.SetDestination(targetDestination);
        }
    }

    internal bool IsTargetExist()
    {
        //if (targetDetector == null)
        //    ColorDebug.GreenLog("HaHaHaHaHaHaHa");

        return targetDetector.IsTargetDetected();
    }

    internal bool IsTargetInAttackRange()
    {
        if (!IsTargetExist()) return false;

        (IPerceivable perceivable, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        return targetInfo.Value.distance <= enemyData.AttackRange;
    }

    private void InformTargetPosition()
    {
        (IPerceivable perceivable, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        onPlayerPositionUpdated?.Invoke(this, targetInfo.Value.perceivable.Transform.position);
    }

    internal void StartInformTargetPositionCoroutine()
    {
        if (informPlayerPositionCoroutine is null)
            informPlayerPositionCoroutine = StartCoroutine(InformTargetPositionCoroutine());
    }

    internal void StopInformTargetPositionCoroutine()
    {
        if (informPlayerPositionCoroutine is not null)
        {
            StopCoroutine(informPlayerPositionCoroutine);
            informPlayerPositionCoroutine = null;
        }
    }

    internal bool HasWaypoint()
    {
        return patrolWaypoints is not null && patrolWaypoints.Length != 0;
    }

    internal void UpdateTargetLostTimer(float deltaTime)
    {
        timeSinceTargetLost += deltaTime;
    }

    internal void ResetTargetLostTimer()
    {
        timeSinceTargetLost = 0f;
    }

    internal bool IsOverTargetLost()
    {
        return timeSinceTargetLost > enemyData.TimeToLostTarget;
    }

    internal void UpdateHideTimer(float deltaTime)
    {
        hideTimer += deltaTime;
    }

    internal void ResetHideTimer()
    {
        hideTimer = 0f;
        hideDuration = UnityEngine.Random.Range(enemyData.MinHideTime, enemyData.MaxHideTime);
    }

    internal void SetHideDuration(float duration)
    {
        hideDuration = duration;
    }

    internal bool IsHideCompleted()
    {
        return hideTimer >= hideDuration;
    }

    internal void ResetPeekTimer()
    {
        peekTimer = 0f;
    }

    internal void UpdatePeekTimer(float deltaTime)
    {
        peekTimer += deltaTime;
    }

    internal bool IsPeekCompleted()
    {
        return peekTimer >= enemyData.ReactionTime;
    }

    internal void SetDestinationOnAgent(Vector3 destination)
    {
        ColorDebug.RedLog($"SetDestinationOnAgent: {destination}");

        targetDestination = destination;
        navMeshAgent.SetDestination(destination);
    }

    internal bool TryFindCover(out CoverPoint bestCover)
    {
        bestCover = targetDetector.FindBestCover();

        if (bestCover is null)
        {
            reservedCoverPoint = null;
            return false;
        }
        else
        {
            if (bestCover.Reserve(gameObject))
            {
                reservedCoverPoint = bestCover;
                return true;
            }
            else
            {
                reservedCoverPoint = null;
                bestCover = null;
                return false;
            }
        }
    }

    internal void ReleaseCover()
    {
        reservedCoverPoint.Release();
        reservedCoverPoint = null;
    }

    internal bool IsAgentArrived()
    {
        return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    internal void SetStateText(string text)
    {
        stateText.text = text;
    }

    internal void SetIsCrouch(bool isCrouch)
    {
        enemyAnimator.SetCrouch(isCrouch);
    }

    internal void SetAttackAnimation()
    {
        enemyAnimator.SetUpperBodyOffset(-0.5f, 0.1f);
        enemyAnimator.SetShoot(true);
    }

    internal void SetIdleAnimation()
    {
        enemyAnimator.SetUpperBodyOffset(0f, 0f);
        enemyAnimator.SetShoot(false);
    }

    internal void SetAttackAnimation(float headOffset, float bodyOffset)
    {
        enemyAnimator.SetUpperBodyOffset(headOffset, bodyOffset);
    }

    internal void SetShoot(bool isShoot)
    {
        enemyAnimator.SetShoot(isShoot);
    }

    internal void ResetUpperBody()
    {
        enemyAnimator.ResetUpperBody();
    }

    private IEnumerator InformTargetPositionCoroutine()
    {
        while (true)
        {
            if (targetDetector.IsTargetDetected())
                InformTargetPosition();
            yield return informPlayerPositionWait;
        }
    }

    private void ScanStarted()
    {
        Debug.Log("ScanStarted");
        if (isPatrolEnemy)
            ChangeState(IEnemyState.IdleState);
    }

    private void ScanCanceled()
    {
        Debug.Log("ScanCanceled");
        if (isPatrolEnemy)
            ChangeState(IEnemyState.PatrolState);
    }

    private void ScanCompleted()
    {
        if (!IsTargetExist())
        {
#if UNITY_EDITOR
            Debug.LogError("ScanCompleted, target is null");
#endif

            return;
        }

        (IPerceivable perceivable, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;
        IPerceivable target = targetInfo.Value.perceivable;

        Debug.Log("ScanCompleted");
        ChangeState(IEnemyState.AttackState);

        onPlayerDetected?.Invoke(this, target, target.Transform.position);
    }

    private IEnumerator TargetDistanceCheckCoroutine()
    {
        while (true)
        {
            foreach ((IPerceivable perceivable, float distance) in targetDetector.VisibleTargets)
            {
                if (distance < enemyData.PrimaryViewRadius)
                {
                    IncreaseAlert(AlertData.MAX);
                }
            }

            yield return targetCheckWait;
        }
    }

    private void StartTargetDistanceCheckCoroutine()
    {
        if (targetCheckCoroutine == null)
            targetCheckCoroutine = StartCoroutine(TargetDistanceCheckCoroutine());
    }

    private void StopTargetDistanceCheckCoroutine()
    {
        if (targetCheckCoroutine != null)
        {
            StopCoroutine(targetCheckCoroutine);
            targetCheckCoroutine = null;
        }
    }

    private void TargetDetected(IPerceivable target)
    {
        StartTargetDistanceCheckCoroutine();
    }

    private void TargetLosted(IPerceivable target)
    {
        // 기존: 전원 놓침
        // 현재: 타겟 1건 놓침!

        onPlayerLosted?.Invoke(this, targetLastKnownPosition);
        StopTargetDistanceCheckCoroutine();
    }

    private IEnumerator IncreaseAlertCoroutine()
    {
        // Target�� Detect �ǰ� �ٽ� 1���þ߷� �����ԵǴ� ��� -> �̰͵� Ž���ؾ� ��!
        WaitForSeconds wait = new WaitForSeconds(AlertData.AlertCheckInterval);
        int increaseAmount = (int)(AlertData.AlertCheckInterval * AlertData.AlertPerSecond);

        while (alertLevel != AlertData.MAX)
        {
            IncreaseAlert(increaseAmount);
            yield return wait;
        }

        // alertLevel Coroutine ���� ���..?
    }

    private void IncreaseAlert(int alertLevel)
    {
        this.alertLevel += alertLevel;
        this.alertLevel = Mathf.Min(this.alertLevel, AlertData.MAX);
    }

    private void DecreaseAlert(int alertLevel)
    {
        this.alertLevel -= alertLevel;
        this.alertLevel = Mathf.Max(this.alertLevel, AlertData.MIN);
    }

    internal void ChangeDefaultState()
    {
        if (isPatrolEnemy) ChangeState(IEnemyState.PatrolState);
        else ChangeState(IEnemyState.IdleState);
    }

    internal void ChangeState(IEnemyState enemyState)
    {
        if (currentState is not null)
        {
            if (currentState == enemyState) return;

            currentState.Exit(this);
        }

        currentState = enemyState;

        currentState.Enter(this);
    }

    public void ReceiveSquadAlert(IPerceivable target, Vector3 lastKnownPosition)
    {
        ColorDebug.Log("ReceiveSquadAlert Change AttackState", Color.red);

        targetLastKnownPosition = lastKnownPosition;

        EnableFieldOfView(false);
        ChangeState(IEnemyState.AttackState);
    }

    public void SetFormationDestination(Vector3 targetDestination, Vector3 lastKnownPosition)
    {
        //ColorDebug.GreenLog($"SetFormationDestination: {targetDestination}");

        this.targetDestination = targetDestination;
        targetLastKnownPosition = lastKnownPosition;

        //ColorDebug.GreenLog($"IsTargetExist: {IsTargetExist()}");

        if (!IsTargetExist())
        {
            SetDestinationOnAgent(targetDestination);
        }
    }

    internal bool RotateTowardTarget()
    {
        (IPerceivable perceivable, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        Vector3 directionToTarget = targetInfo.Value.perceivable.Transform.position - muzzle.position;
        directionToTarget.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        Quaternion calibration = Quaternion.Inverse(transform.rotation) * muzzle.rotation;

        Quaternion finalEnemyRotation = targetRotation * Quaternion.Inverse(calibration);

        transform.rotation = Quaternion.Slerp(transform.rotation, finalEnemyRotation, enemyData.RotateSpeed * Time.deltaTime);

        Vector3 currentMuzzleDir = muzzle.forward;
        currentMuzzleDir.y = 0f;

        float angleDifference = Vector3.Angle(currentMuzzleDir, directionToTarget);

        return angleDifference < 1f;
    }

    private void EnemyDied()
    {
        GetComponent<Collider>().enabled = false;
        navMeshAgent.enabled = false;
        enabled = false;

        ChangeState(IEnemyState.DeadState);
    }

    [ContextMenu("ChangeStateImmediately")]
    private void ChangeStateImmediately()
    {
        ChangeState(IEnemyState.CoverState);
    }

    [ContextMenu("SetDestination")]
    private void SetDestinationImmediately()
    {
        navMeshAgent.SetDestination(new Vector3(14f, 0f, 10f));
        ChangeState(IEnemyState.CoverState);
    }
}
