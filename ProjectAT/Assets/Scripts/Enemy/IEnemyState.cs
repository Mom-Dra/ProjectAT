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

    void TargetDetected(Enemy enemy, IPerceivable target) { }
    void TargetConfirmed(Enemy enemy, IPerceivable target) { }
    void TargetLost(Enemy enemy, IPerceivable target) { }
    void OrderReceived(Enemy enemy, SquadOrder order) { }
}

public class EnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.stateText.text = "Idle";
        enemy.EnableFieldOfView(true);
        enemy.SetAttackMode(false);
        enemy.StopMoving();
    }

    public void Update(Enemy enemy)
    {

    }

    public void Exit(Enemy enemy)
    {

    }

    public void TargetConfirmed(Enemy enemy, IPerceivable target)
    {
        enemy.ChangeState(IEnemyState.AttackState);
    }

    public void OrderReceived(Enemy enemy, SquadOrder squadOrder)
    {
        switch (squadOrder.OrderKind)
        {
            case OrderKind.Attack:
                enemy.ChangeState(IEnemyState.ChaseState);
                break;

            case OrderKind.Search:
                enemy.ChangeState(IEnemyState.SearchState);
                break;

            case OrderKind.Patrol:
                enemy.ChangeState(IEnemyState.PatrolState);
                break;
        }
    }
}

public class EnemyDeadState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.EnableFieldOfView(false);
        enemy.SetAttackMode(false);
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
        enemy.stateText.text = "Patrol";
        enemy.EnableFieldOfView(true);
        enemy.SetAttackMode(false);

        if (enemy.UseOrderedDestination)
        {
            MoveToOrderedDestination(enemy);
            return;
        }

        if (!enemy.HasPatrolWaypoints)
        {
            enemy.ChangeState(IEnemyState.IdleState);
            return;
        }

        MoveToCurrentWaypoint(enemy);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.PerceptionSystem.HasAnyTarget)
        {
            enemy.PatrolPausedByTarget = true;
            enemy.StopMoving();
            return;
        }

        if (enemy.PatrolPausedByTarget)
        {
            enemy.PatrolPausedByTarget = false;
            ResumePatrol(enemy);
            return;
        }

        if (!enemy.HasArrived()) return;

        if (!enemy.UseOrderedDestination)
        {
            enemy.WaypointIndex = (enemy.WaypointIndex + 1) % enemy.Waypoints.Length;
            MoveToCurrentWaypoint(enemy);
        }
        else
        {
            enemy.UseOrderedDestination = false;

            if (enemy.HasPatrolWaypoints)
                MoveToCurrentWaypoint(enemy);
            else
                enemy.ChangeState(IEnemyState.IdleState);
        }
    }

    public void Exit(Enemy enemy)
    {

    }

    public void TargetConfirmed(Enemy enemy, IPerceivable target)
    {
        enemy.ChangeState(IEnemyState.AttackState);
    }

    public void TargetDetected(Enemy enemy, IPerceivable target)
    {
        enemy.PatrolPausedByTarget = true;
        enemy.StopMoving();
    }

    public void OrderReceived(Enemy enemy, SquadOrder squadOrder)
    {
        switch (squadOrder.OrderKind)
        {
            case OrderKind.Attack:
                enemy.ChangeState(IEnemyState.AttackState);
                break;

            case OrderKind.Search:
                enemy.ChangeState(IEnemyState.SearchState);
                break;

            case OrderKind.Patrol:
                enemy.UseOrderedDestination = true;
                MoveToOrderedDestination(enemy);
                break;
        }
    }

    private void MoveToCurrentWaypoint(Enemy enemy)
    {
        Transform wayPoint = enemy.Waypoints[enemy.WaypointIndex];

        if (NavMesh.SamplePosition(wayPoint.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            enemy.MoveTo(hit.position);
    }

    private void MoveToOrderedDestination(Enemy enemy)
    {
        Vector3 destination = enemy.CurrentOrderDestination;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            enemy.MoveTo(hit.position);
        else enemy.MoveTo(destination);
    }

    private void ResumePatrol(Enemy enemy)
    {
        if (enemy.UseOrderedDestination)
        {
            MoveToOrderedDestination(enemy);
            return;
        }

        if (enemy.HasPatrolWaypoints)
            MoveToCurrentWaypoint(enemy);
        else
            enemy.ChangeState(IEnemyState.IdleState);
    }
}

public class EnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.stateText.text = "Chase";
        enemy.EnableFieldOfView(false);
        enemy.SetAttackMode(true);

        enemy.MoveTo(enemy.CurrentOrderDestination);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.HasArrived())
        {
            enemy.ChangeState(IEnemyState.SearchState);
        }
    }

    public void Exit(Enemy enemy)
    {

    }

    public void TargetConfirmed(Enemy enemy, IPerceivable target)
    {
        enemy.ChangeState(IEnemyState.AttackState);
    }

    public void OrderReceived(Enemy enemy, SquadOrder squadOrder)
    {
        switch (squadOrder.OrderKind)
        {
            case OrderKind.Attack:
            case OrderKind.Search:
            case OrderKind.Patrol:
                break;

            case OrderKind.Disengage:
                enemy.ChangeState(IEnemyState.IdleState);
                break;
        }
    }
}

public class EnemyAttackState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.stateText.text = "Attack";
        enemy.EnableFieldOfView(false);
        enemy.SetAttackMode(true);

        MoveToDestination(enemy);
    }

    public void Update(Enemy enemy)
    {
        if (enemy.CurrentTarget is null || !enemy.CurrentTarget.IsValidTarget)
        {
            enemy.ChangeState(IEnemyState.SearchState);
            return;
        }

        if (enemy.IsTargetInAttackRange())
        {
            enemy.StopMoving();

            bool aimed = enemy.RotateTowardTarget();

            if (aimed) enemy.Fire();
        }
    }

    public void Exit(Enemy enemy)
    {

    }

    public void TargetLost(Enemy enemy, IPerceivable target)
    {
        enemy.ChangeState(IEnemyState.SearchState);
    }

    public void OrderReceived(Enemy enemy, SquadOrder squadOrder)
    {
        switch (squadOrder.OrderKind)
        {
            case OrderKind.Attack:
                // if (enemy.CurrentTarget is not null && enemy.CurrentTarget.IsValidTarget && !ReferenceEquals(enemy.CurrentTarget, squadOrder.Target))
                // {
                //     // 공격 중인데 타겟이 달라
                //     // 거부
                // }



                // 공격중에 또 공격 명령.. 거부
                break;

            case OrderKind.Search:
            case OrderKind.Patrol:
                break;

            case OrderKind.Disengage:
                enemy.ChangeState(IEnemyState.IdleState);
                break;
        }
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

    private void MoveToDestination(Enemy enemy)
    {
        Vector3 dest = enemy.CurrentOrderDestination;
        if (dest == Vector3.zero)
        {
            // Order 없음 → 타겟 위치로 직접 접근
            var target = enemy.CurrentTarget;
            if (target != null && target.IsValidTarget)
                dest = target.Transform.position;
            else return;
        }

        if (NavMesh.SamplePosition(dest, out var hit, 3f, NavMesh.AllAreas))
            enemy.MoveTo(hit.position);
        else
            enemy.MoveTo(dest);
    }
}

public class EnemySearchState : IEnemyState
{
    internal enum Phase { Moving, Waiting }

    public void Enter(Enemy enemy)
    {
        enemy.stateText.text = "Search";

        enemy.EnableFieldOfView(false);
        enemy.SetAttackMode(true);

        enemy.SearchCenter = ResolveCenter(enemy);
    }

    public void Update(Enemy enemy)
    {
        switch (enemy.Phase)
        {
            case Phase.Moving:
                if (enemy.HasArrived())
                {
                    enemy.StopMoving();
                    enemy.Phase = Phase.Waiting;
                    enemy.WaitTimer = 0f;
                }
                break;

            case Phase.Waiting:
                enemy.WaitTimer += Time.deltaTime;

                if (enemy.WaitTimer >= enemy.EnemyData.SearchPointWaitTime)
                    PickNextPointAndMove(enemy);
                break;
        }
    }

    public void Exit(Enemy enemy)
    {

    }

    public void TargetConfirmed(Enemy enemy, IPerceivable target)
    {
        enemy.ChangeState(IEnemyState.AttackState);
    }

    public void OrderReceived(Enemy enemy, SquadOrder squadOrder)
    {
        switch (squadOrder.OrderKind)
        {
            case OrderKind.Attack:
                enemy.ChangeState(IEnemyState.ChaseState);
                break;

            case OrderKind.Search:
                enemy.SearchCenter = ResolveCenter(enemy);
                PickNextPointAndMove(enemy);
                break;

            case OrderKind.Patrol:
                enemy.ChangeState(IEnemyState.PatrolState);
                break;

            case OrderKind.Disengage:
                enemy.ChangeState(IEnemyState.IdleState);
                break;
        }
    }

    private static Vector3 ResolveCenter(Enemy enemy)
    {
        Vector3 dest = enemy.CurrentOrderDestination;
        if (dest != Vector3.zero) return dest;
        return enemy.LastKnownPosition;
    }

    private static Vector3 GetRandomOffset(float radius)
    {
        Vector2 c = Random.insideUnitCircle * radius;
        return new Vector3(c.x, 0f, c.y);
    }

    private void GoLastKnownPosition(Enemy enemy)
    {
        enemy.CurrentPoint = enemy.LastKnownPosition;
        enemy.MoveTo(enemy.LastKnownPosition);
        enemy.Phase = Phase.Moving;
    }

    private void PickNextPointAndMove(Enemy enemy)
    {
        for (int i = 0; i < enemy.EnemyData.SearchMaxAttempts; ++i)
        {
            Vector3 candidate = enemy.SearchCenter + GetRandomOffset(enemy.EnemyData.SearchRadius);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, enemy.EnemyData.SearchNavSampleRadius, NavMesh.AllAreas))
            {
                enemy.CurrentPoint = hit.position;
                enemy.MoveTo(hit.position);
                enemy.Phase = Phase.Moving;
                return;
            }
        }

        if (NavMesh.SamplePosition(enemy.SearchCenter, out NavMeshHit navMeshHit, enemy.EnemyData.SearchNavSampleRadius, NavMesh.AllAreas))
        {
            enemy.CurrentPoint = navMeshHit.position;
            enemy.MoveTo(navMeshHit.position);
            enemy.Phase = Phase.Moving;
        }
        else
        {
            enemy.Phase = Phase.Waiting;
            enemy.WaitTimer = 0f;
        }
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
