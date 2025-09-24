using UnityEngine;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;

public interface IEnemyState
{
    static readonly IEnemyState ServerEnemyIdleState = new ServerEnemyIdleState();
    static readonly IEnemyState ServerEnemyPatrolState = new ServerEnemyPatrolState();
    static readonly IEnemyState ServerEnemyAttackState = new ServerEnemyAttackState();
    static readonly IEnemyState ServerEnemyChaseState = new ServerEnemyChaseState();
    static readonly IEnemyState ServerEnemyWonderState = new ServerEnemyWonderState();
    static readonly IEnemyState ClientEnemyIdleState = new ClientEnemyIdleState();
    static readonly IEnemyState ClientEnemyPatrolState = new ClientEnemyPatrolState();
    static readonly IEnemyState ClientEnemyAttackState = new ClientEnemyAttackState();
    static readonly IEnemyState ClientEnemyChaseState = new ClientEnemyChaseState();
    static readonly IEnemyState ClientEnemyWonderState = new ClientEnemyWonderState();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
    // void Attack(Enemy enemy);
}

public abstract class EnemyAttackState : IEnemyState
{
    public void Attack(Enemy enemy)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfViewNetcode(false);
    }

    public abstract void Update(Enemy enemy);

    public virtual void Exit(Enemy enemy)
    {
        // enemy.GetFieldOfViewNetcode().enabled = true;
    }
}

public abstract class EnemyIdleState : IEnemyState
{
    public virtual void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfViewNetcode(true);
    }

    public abstract void Update(Enemy enemy);

    public abstract void Exit(Enemy enemy);
}

public abstract class EnemyPatrolState : IEnemyState
{
    public virtual void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfViewNetcode(true);
    }

    public abstract void Update(Enemy enemy);

    public abstract void Exit(Enemy enemy);
}

public class ServerEnemyIdleState : EnemyIdleState
{
    public override void Enter(Enemy enemy)
    {
        base.Enter(enemy);

        Debug.Log("Entering Server Enemy Idle State");

        Debug.Log("enemy.SetBehaviorGraphAgentState(Enemy_State.Idle)");
        enemy.SetBehaviorGraphAgentState(Enemy_State.Idle);
    }

    public override void Update(Enemy enemy)
    {
        if (enemy.AlertLevel >= enemy.AlertData.AlertThreshold)
            enemy.ChangeState(Enemy_State.Search);
        else if (enemy.AlertLevel >= enemy.AlertData.CombatThreshold)
            enemy.ChangeState(Enemy_State.Chase);
    }

    public override void Exit(Enemy enemy)
    {

    }
}

public class ServerEnemyPatrolState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Server Enemy Patrol State");
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

// Chase와 Attack이 사실상 Attack State임!
public class ServerEnemyAttackState : EnemyAttackState
{
    public override void Enter(Enemy enemy)
    {
        base.Enter(enemy);

        Debug.Log("Entering Server Enemy Attack State");
        enemy.SetBehaviorGraphAgentState(Enemy_State.Attack);
    }

    public override void Update(Enemy enemy)
    {
        // 시야에 없으면 감소!
    }

    public override void Exit(Enemy enemy)
    {
        base.Exit(enemy);
    }
}

public class ServerEnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Server Enemy Chase State");
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

// 이거 Search State로 변경하자!
public class ServerEnemyWonderState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Server Enemy Wonder State");
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

public class ClientEnemyIdleState : EnemyIdleState
{
    public override void Enter(Enemy enemy)
    {
        base.Enter(enemy);
        Debug.Log("Entering Client Enemy Idle State");
    }

    public override void Update(Enemy enemy)
    {

    }

    public override void Exit(Enemy enemy)
    {

    }
}

public class ClientEnemyPatrolState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Client Enemy Patrol State");
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

public class ClientEnemyAttackState : EnemyAttackState
{
    public override void Enter(Enemy enemy)
    {
        base.Enter(enemy);
        Debug.Log("Entering Client Enemy Attack State");
    }

    public override void Update(Enemy enemy)
    {

    }

    public override void Exit(Enemy enemy)
    {
        base.Exit(enemy);
    }
}

public class ClientEnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Client Enemy Chase State");
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

public class ClientEnemyWonderState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Client Enemy Wonder State");
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}