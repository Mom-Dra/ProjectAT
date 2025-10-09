using UnityEngine;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;

public interface IEnemyState
{
    static readonly IEnemyState ServerEnemyIdleState = new EnemyIdleState();
    static readonly IEnemyState ServerEnemyPatrolState = new EnemyPatrolState();
    static readonly IEnemyState ServerEnemyAttackState = new EnemyAttackState();
    static readonly IEnemyState ServerEnemyChaseState = new EnemyChaseState();
    static readonly IEnemyState ServerEnemyWonderState = new EnemySearchState();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}

public class EnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfViewNetcode(true);
        enemy.SetBehaviorGraphAgentState(Enemy_State.Idle);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.AlertLevel >= enemy.AlertData.AlertThreshold)
            enemy.ChangeState(Enemy_State.Search);
        else if (enemy.AlertLevel >= enemy.AlertData.CombatThreshold)
            enemy.ChangeState(Enemy_State.Chase);
    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemyPatrolState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfViewNetcode(true);
        enemy.SetBehaviorGraphAgentState(Enemy_State.Patrol);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.AlertLevel >= enemy.AlertData.AlertThreshold)
            enemy.ChangeState(Enemy_State.Search);
        else if (enemy.AlertLevel >= enemy.AlertData.CombatThreshold)
            enemy.ChangeState(Enemy_State.Chase);
    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.SetBehaviorGraphAgentState(Enemy_State.Chase);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.AlertLevel < enemy.AlertData.CombatThreshold)
            enemy.ChangeState(Enemy_State.Search);
    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemyAttackState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfViewNetcode(false);
        enemy.SetBehaviorGraphAgentState(Enemy_State.Attack);
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemySearchState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.SetBehaviorGraphAgentState(Enemy_State.Search);
        enemy.Time = 0f;
    }

    public void Exit(Enemy enemy)
    {

    }

    public void Update(Enemy enemy)
    {
        enemy.Time += Time.deltaTime;

        if (enemy.Time >= Enemy.WONDERTIME)
            enemy.ChangeDefaultState();
    }
}
