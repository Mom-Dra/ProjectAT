using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;
using MomDra;

public abstract class Enemy : LivingEntity, IAttackable
{
    protected BehaviorGraphAgent behaviorGraphAgent;
    protected FieldOfViewNetcode fieldOfViewNetcode;

    protected IEnemyState currentState;
    private Weapon weapon;

    // Inspector에서 설정
    [SerializeField]
    private bool isPatrolEnemy;

    internal float Time;
    internal const float WONDERTIME = 10f;

    private void Awake()
    {
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        fieldOfViewNetcode = GetComponent<FieldOfViewNetcode>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            fieldOfViewNetcode.onScanCompleted += ScanCompleted;
            fieldOfViewNetcode.onScanCanceled += ScanCanceled;
            fieldOfViewNetcode.onScanStarted += ScanStarted;

            if (isPatrolEnemy) ChangeState(Enemy_State.Patrol);
        }
        else
        {
            behaviorGraphAgent.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
        {

        }
    }

    internal void SetBehaviorGraphAgentState(Enemy_State enemyState)
    {
        behaviorGraphAgent.SetVariableValue("Enemy_State", enemyState);
    }

    internal void EnableFieldOfViewNetcode(bool enabled)
    {
        fieldOfViewNetcode.enabled = enabled;
    }

    public void Attack()
    {
        // currentState.Attack(this);

        // Enemy 상태 안에서 call 해야 할듯?
        weapon.AttackRpc();
    }

    private void ScanStarted()
    {
        if (isPatrolEnemy)
            ChangeState(Enemy_State.Idle);
    }

    private void ScanCanceled()
    {
        if (isPatrolEnemy)
            ChangeState(Enemy_State.Patrol);
    }

    private void ScanCompleted()
    {
        Debug.Log("ScanCompleted");
        ChangeState(Enemy_State.Attack);
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
            case Enemy_State.Wander:
                currentState = IEnemyState.ServerEnemyWonderState;
                break;
        }

        currentState.Enter(this);
        ChangeStateRpc(enemyState);
    }

    [Rpc(SendTo.NotServer)]
    private void ChangeStateRpc(Enemy_State enemyState)
    {
        if (currentState != null)
            currentState.Exit(this);

        switch (enemyState)
        {
            case Enemy_State.Idle:
                currentState = IEnemyState.ClientEnemyIdleState;
                break;
            case Enemy_State.Patrol:
                currentState = IEnemyState.ClientEnemyPatrolState;
                break;
            case Enemy_State.Attack:
                currentState = IEnemyState.ClientEnemyAttackState;
                break;
            case Enemy_State.Chase:
                currentState = IEnemyState.ClientEnemyChaseState;
                break;
            case Enemy_State.Wander:
                currentState = IEnemyState.ClientEnemyWonderState;
                break;
        }

        currentState.Enter(this);
    }
}
