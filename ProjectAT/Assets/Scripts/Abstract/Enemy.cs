using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;

public abstract class Enemy : LivingEntity
{
    [SerializeField]
    protected BehaviorGraphAgent behaviorGraphAgent;

    protected IEnemyState currentState;

    private void Awake()
    {
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
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

    internal BehaviorGraphAgent GetBehaviorGraphAgent()
    {
        return behaviorGraphAgent;
    }

    public void ChangeState(Enemy_State enemyState)
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
        }

        currentState.Enter(this);
        ChangeStateRpc(enemyState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChangeStateRpc(Enemy_State enemyState)
    {
        if (IsServer) return;

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
        }

        currentState.Enter(this);
    }
}
