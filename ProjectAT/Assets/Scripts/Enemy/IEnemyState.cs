using UnityEngine;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;

public interface IEnemyState
{
    static readonly IEnemyState ServerEnemyIdleState = new ServerEnemyIdleState();
    static readonly IEnemyState ServerEnemyPatrolState = new ServerEnemyPatrolState();
    static readonly IEnemyState ServerEnemyAttackState = new ServerEnemyAttackState();
    static readonly IEnemyState ClientEnemyIdleState = new ClientEnemyIdleState();
    static readonly IEnemyState ClientEnemyPatrolState = new ClientEnemyPatrolState();
    static readonly IEnemyState ClientEnemyAttackState = new ClientEnemyAttackState();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}

public abstract class EnemyAttackState : IEnemyState
{
    public virtual void Enter(Enemy enemy)
    {
        enemy.GetFieldOfViewNetcode().enabled = false;
    }

    public virtual void Exit(Enemy enemy)
    {
        enemy.GetFieldOfViewNetcode().enabled = true;
    }

    public abstract void Update(Enemy enemy);
}

public class ServerEnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Server Enemy Idle State");
        enemy.GetBehaviorGraphAgent().SetVariableValue("Enemy_State", Enemy_State.Idle);
    }

    public void Exit(Enemy enemy)
    {

    }

    public void Update(Enemy enemy)
    {
        // Logic for updating Idle state
    }
}

public class ServerEnemyPatrolState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Server Enemy Patrol State");
        enemy.GetBehaviorGraphAgent().SetVariableValue("Enemy_State", Enemy_State.Patrol);
    }

    public void Update(Enemy enemy)
    {
        // Logic for updating Patrol state
    }

    public void Exit(Enemy enemy)
    {
        // Logic for exiting Patrol state
    }
}

public class ServerEnemyAttackState : EnemyAttackState
{
    public override void Enter(Enemy enemy)
    {
        base.Enter(enemy);
        Debug.Log("Entering Server Enemy Attack State");
        enemy.GetBehaviorGraphAgent().SetVariableValue("Enemy_State", Enemy_State.Attack);
    }

    public override void Update(Enemy enemy)
    {
        // Logic for updating Attack state
    }

    public override void Exit(Enemy enemy)
    {
        base.Exit(enemy);
        // Logic for exiting Attack state
    }
}

public class ClientEnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Debug.Log("Entering Client Enemy Idle State");
    }

    public void Exit(Enemy enemy)
    {

    }

    public void Update(Enemy enemy)
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
        // Logic for updating Patrol state
    }

    public void Exit(Enemy enemy)
    {
        // Logic for exiting Patrol state
    }
}

public class ClientEnemyAttackState : EnemyAttackState
{
    public override void Enter(Enemy enemy)
    {
        base.Enter(enemy);
        Debug.Log("Entering Client Enemy Attack State");
        // Logic for entering Attack state
    }

    public override void Update(Enemy enemy)
    {
        // Logic for updating Attack state
    }

    public override void Exit(Enemy enemy)
    {
        base.Exit(enemy);
        // Logic for exiting Attack state
        enemy.GetFieldOfViewNetcode().enabled = true;
    }
}