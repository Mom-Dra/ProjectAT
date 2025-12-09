using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;
using MomDra;
using Unity.Collections;
using System.Collections;
using Unity.VisualScripting;
using System;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.InputSystem.XR.Haptics;
using static UnityEngine.EventSystems.EventTrigger;
using TMPro;
using MomDra.Weapon;

public abstract class Enemy : LivingEntity, IAttackable, ISquadMember
{
    public event Action<ISquadMember, Transform, Vector3> onPlayerDetected;
    public event Action<ISquadMember, Vector3> onPlayerLosted;
    public event Action<ISquadMember, Vector3> onPlayerPositionUpdated;

    internal int CurrentWaypointIndex;

    internal ICoverSubState CurrCoverSubState;

    protected NavMeshAgent navMeshAgent;
    protected FieldOfViewVisuals fieldOfViewVisual;
    protected TargetDetector targetDetector;
    protected EnemyAnimator enemyAnimator;
    protected IEnemyState currentState;

    // Inspector에서 설정
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

    // Debug용 변수
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
    private Transform currentTarget;
    private Transform muzzle;

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
        if(navMeshAgent.destination != targetDestination)
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

        (Transform transform, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        if (targetInfo.Value.distance <= enemyData.AttackRange) return true;

        return false;
    }

    private void InformTargetPosition()
    {
        (Transform transform, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        onPlayerPositionUpdated?.Invoke(this, targetInfo.Value.transform.position);
    }

    internal void StartInformTargetPositionCoroutine()
    {
        if (informPlayerPositionCoroutine is null)
            informPlayerPositionCoroutine = StartCoroutine(InformTargetPositionCoroutine());
    }

    internal void StopInformTargetPositionCoroutine()
    {
        if(informPlayerPositionCoroutine is not null)
        {
            StopCoroutine(informPlayerPositionCoroutine);
            informPlayerPositionCoroutine = null;
        }
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
        while(true)
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
        if(!IsTargetExist())
        {
            Debug.LogWarning("ScanCompleted, target is null");
            //ColorDebug.Log("ScanCompleted, target is null", Color.red);

#if UNITY_EDITOR
            throw new Exception("ScanCompleted, target is null");
#endif
        }

        (Transform transform, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        Debug.Log("ScanCompleted");
        ChangeState(IEnemyState.AttackState);

        onPlayerDetected?.Invoke(this, targetInfo.Value.transform, targetInfo.Value.transform.position);
    }

    private IEnumerator TargetDistanceCheckCoroutine()
    {
        while(true)
        {
            foreach ((Transform transform, float distance) in targetDetector.VisibleTargets)
            {
                // 1차 시야 안에 있을 경우
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
        if(targetCheckCoroutine != null)
        {
            StopCoroutine(targetCheckCoroutine);
            targetCheckCoroutine = null;
        }
    }

    private void TargetDetected()
    {
        // Todo
        // 1차 시야 인지 판별
        // Target이 Detected 되었다는건 2차 시야 안에 있다는 것!
        StartTargetDistanceCheckCoroutine();
    }

    private void TargetLosted()
    {
        onPlayerLosted?.Invoke(this, targetLastKnownPosition);
        StopTargetDistanceCheckCoroutine();
    }

    private IEnumerator IncreaseAlertCoroutine()
    {
        // Target이 Detect 되고 다시 1차시야로 들어오게되는 경우 -> 이것도 탐지해야 함!
        WaitForSeconds wait = new WaitForSeconds(AlertData.AlertCheckInterval);
        int increaseAmount = (int)(AlertData.AlertCheckInterval * AlertData.AlertPerSecond);

        while (alertLevel != AlertData.MAX)
        {
            IncreaseAlert(increaseAmount);
            yield return wait;
        }

        // alertLevel Coroutine 변수 사용..?
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

    public void ReceiveSquadAlert(Transform target, Vector3 lastKnownPosition)
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
        (Transform transform, float distance)? targetInfo = targetDetector.GetFirstTargetInfo;

        Vector3 directionToTarget = targetInfo.Value.transform.position - muzzle.position;
        directionToTarget.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
        Quaternion calibration = Quaternion.Inverse(transform.rotation) * muzzle.rotation;

        Quaternion finalEnemyRotation = targetRotation * Quaternion.Inverse(calibration);

        transform.rotation = Quaternion.Slerp(transform.rotation, finalEnemyRotation, enemyData.RotateSpeed * Time.deltaTime);

        Vector3 currentMuzzleDir = muzzle.forward;
        currentMuzzleDir.y = 0f;

        float angleDifference = Vector3.Angle(currentMuzzleDir, directionToTarget);

        if (angleDifference < 1f) return true;

        return false;
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
