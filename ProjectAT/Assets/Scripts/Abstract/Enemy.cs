using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;
using MomDra;
using Unity.Collections;
using System.Collections;
using Unity.VisualScripting;

public abstract class Enemy : LivingEntity, IAttackable
{
    protected BehaviorGraphAgent behaviorGraphAgent;
    //protected FieldOfViewNetcode fieldOfViewNetcode;

    protected FieldOfView fieldOfView;
    // 이 것만 Netcode와 아닌거 구분하자

    protected IEnemyState currentState;
    private Weapon weapon;

    // Inspector에서 설정
    [SerializeField]
    private bool isPatrolEnemy;

    [SerializeField]
    private EnemyData enemyData;
    public EnemyData EnemyData => enemyData;

    [SerializeField]
    private AlertData alertData;
    public AlertData AlertData => alertData;

    [SerializeField]
    private float targetCheckInterval = 0.2f;
    private WaitForSeconds targetCheckWait;

    internal float Time;
    internal const float WONDERTIME = 10f;

    [SerializeField]
    private int alertLevel;
    public int AlertLevel => alertLevel;

    // Debug용 변수
    [SerializeField]
    private Enemy_State enemyState;

    private Coroutine targetCheckCoroutine;

    private void Awake()
    {
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        fieldOfView = GetComponent<FieldOfView>();

        fieldOfView.SetEnemyData(this);
        fieldOfView.onScanComplete += ScanCompleted;
        fieldOfView.onScanCancel += ScanCanceled;
        fieldOfView.onScanStart += ScanStarted;
        fieldOfView.onTargetDetect += TargetDetected;
        fieldOfView.onTargetLosted += TargetLosted;

        behaviorGraphAgent.SetVariableValue("attackRange", enemyData.AttackRange);
        behaviorGraphAgent.SetVariableValue("rotateSpeed", enemyData.RotateSpeed);
        behaviorGraphAgent.SetVariableValue("muzzle", transform.FindChildRecursive("FX_Shoot_01_muzzle"));
        behaviorGraphAgent.SetVariableValue("searchRadius", enemyData.SearchRadius);
        behaviorGraphAgent.SetVariableValue("weapon", GetComponentInChildren<Weapon>());

        targetCheckWait = new WaitForSeconds(targetCheckInterval);

        if (isPatrolEnemy) ChangeState(Enemy_State.Patrol);
    }

    internal void SetBehaviorGraphAgentState(Enemy_State enemyState)
    {
        behaviorGraphAgent.SetVariableValue("Enemy_State", enemyState);
    }

    internal void EnableFieldOfViewNetcode(bool enabled)
    {
        fieldOfView.enabled = enabled;
    }

    public void Attack()
    {
        // currentState.Attack(this);

        // Enemy 상태 안에서 call 해야 할듯?
        weapon.Attack();
    }

    private void ScanStarted()
    {
        Debug.Log("ScanStarted");
        if (isPatrolEnemy)
            ChangeState(Enemy_State.Idle);
    }

    private void ScanCanceled()
    {
        Debug.Log("ScanCanceled");
        if (isPatrolEnemy)
            ChangeState(Enemy_State.Patrol);
    }

    private void ScanCompleted()
    {
        Debug.Log("ScanCompleted");
        ChangeState(Enemy_State.Attack);
    }

    private IEnumerator TargetDistanceCheckCoroutine()
    {
        while(true)
        {
            foreach ((Transform transform, float distance) in fieldOfView.VisibleTargets)
            {
                // 1차 시야 안에 있을 경우
                if (distance < enemyData.PrimaryViewRadius)
                    IncreaseAlert(AlertData.MAX);
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
        if (isPatrolEnemy) ChangeState(Enemy_State.Patrol);
        else ChangeState(Enemy_State.Idle);
    }

    internal void ChangeState(Enemy_State enemyState)
    {
        if (currentState != null)
            currentState.Exit(this);

        this.enemyState = enemyState;

        switch (enemyState)
        {
            case Enemy_State.Idle:
                currentState = IEnemyState.ServerEnemyIdleState;
                break;
            case Enemy_State.Patrol:
                currentState = IEnemyState.ServerEnemyPatrolState;
                break;
            case Enemy_State.Attack:
                currentState = IEnemyState.ServerEnemyAttackState;
                break;
            case Enemy_State.Chase:
                currentState = IEnemyState.ServerEnemyChaseState;
                break;
            case Enemy_State.Search:
                currentState = IEnemyState.ServerEnemyWonderState;
                break;
        }

        currentState.Enter(this);
    }
}
