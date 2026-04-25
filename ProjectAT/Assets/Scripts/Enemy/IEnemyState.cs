using UnityEngine;
using UnityEngine.AI;

public interface IEnemyState
{
    static readonly IEnemyState IdleState = new EnemyIdleState();
    static readonly IEnemyState DeadState = new EnemyDeadState();
    static readonly IEnemyState PatrolState = new EnemyPatrolState();
    static readonly IEnemyState AttackState = new EnemyAttackState();
    static readonly IEnemyState ChaseState = new EnemyChaseState();
    static readonly IEnemyState SearchState = new EnemySearchState();
    // static readonly IEnemyState CoverState = new EnemyCoverState();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}

public class EnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.StopMoving();
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemyDeadState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.StopMoving();
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemyPatrolState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        if (NavMesh.SamplePosition(enemy.CurrentOrderDestination, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            enemy.MoveTo(hit.position);
        else enemy.MoveTo(enemy.CurrentOrderDestination);
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

public class EnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {

    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }

    private void HandleTargetTracking(Enemy enemy)
    {

    }

    private void HandleTargetLost(Enemy enemy)
    {

    }
}

public class EnemyAttackState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        MoveToSlot(enemy);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.IsTargetInAttackRange())
        {
            enemy.StopMoving();
            enemy.Fire();
        }
        else
        {
            MoveToSlot(enemy);
        }
    }

    public void Exit(Enemy enemy)
    {

    }

    private void MoveToSlot(Enemy enemy)
    {
        // Enemy Class에 있어야 하는게 아닌가?

        Vector3 slot = enemy.CurrentOrderDestination;
        if (slot == Vector3.zero) slot = enemy.LastKnownPosition;

        if (NavMesh.SamplePosition(slot, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            enemy.MoveTo(hit.position);
        else enemy.MoveTo(slot);
    }
}

public class EnemySearchState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        Vector3 point = enemy.CurrentOrderDestination;
        if (point == Vector3.zero) point = enemy.LastKnownPosition;

        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            enemy.MoveTo(hit.position);
        else enemy.MoveTo(point);
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }
}

// public class EnemyCoverState : IEnemyState
// {
//     public void Enter(Enemy enemy)
//     {

//     }

//     public void Update(Enemy enemy)
//     {

//     }

//     public void Exit(Enemy enemy)
//     {

//     }

//     internal void ChangeSubState(Enemy enemy, ICoverSubState coverSubState)
//     {

//     }
// }

// public interface ICoverSubState
// {
//     static readonly ICoverSubState MoveState = new EnemyCoverMoveState();
//     static readonly ICoverSubState HideState = new EnemyCoverHideState();
//     static readonly ICoverSubState PeekState = new EnemyCoverPeekState();
//     static readonly ICoverSubState AttackState = new EnemyCoverAttackState();

//     void Enter(Enemy enemy, EnemyCoverState enemyCoverState);
//     void Update(Enemy enemy, EnemyCoverState enemyCoverState);
//     void Exit(Enemy enemy, EnemyCoverState enemyCoverState);
// }

// public class EnemyCoverMoveState : ICoverSubState
// {
//     public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.SetStateText("CoverMove");
//         enemy.SetDestinationOnAgent(enemy.ReservedCoverPoint.transform.position);
//     }

//     public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         if (enemy.IsAgentArrived())
//         {
//             enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
//         }
//     }

//     public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
//     {

//     }
// }

// public class EnemyCoverHideState : ICoverSubState
// {
//     public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.SetStateText("CoverHide");
//         enemy.ResetHideTimer();
//         enemy.SetIsCrouch(true);
//     }

//     public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.UpdateHideTimer(Time.deltaTime);

//         if (enemy.IsHideCompleted())
//         {
//             if (!enemy.IsTargetExist())
//             {
//                 enemy.ChangeState(IEnemyState.ChaseState);
//             }

//             enemyCoverState.ChangeSubState(enemy, ICoverSubState.PeekState);
//         }
//     }

//     public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.SetIsCrouch(false);
//     }
// }

// public class EnemyCoverPeekState : ICoverSubState
// {
//     public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.SetStateText("CoverPeek");
//         enemy.ResetPeekTimer();
//     }

//     public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.UpdateHideTimer(Time.deltaTime);

//         if (!enemy.IsTargetExist())
//         {
//             enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
//             return;
//         }

//         if (enemy.IsTargetInAttackRange())
//         {
//             enemyCoverState.ChangeSubState(enemy, ICoverSubState.AttackState);
//         }
//     }

//     public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
//     {

//     }
// }

// public class EnemyCoverAttackState : ICoverSubState
// {
//     public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         enemy.SetStateText("CoverAttack");
//     }

//     public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
//     {
//         if (!enemy.IsTargetExist())
//         {
//             enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
//             return;
//         }

//         enemy.StartInformTargetPositionCoroutine();
//         enemy.ResetTargetLostTimer();

//         if (!enemy.IsTargetInAttackRange())
//             return;

//         if (enemy.IsReloading())
//         {
//             enemy.SetIdleAnimation();
//             enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
//             return;
//         }

//         bool rotationComplete = enemy.RotateTowardTarget();

//         if (!rotationComplete)
//         {
//             enemy.SetIdleAnimation();
//             return;
//         }

//         enemy.Attack();
//         enemy.SetAttackAnimation();
//     }

//     public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
//     {

//     }
// }