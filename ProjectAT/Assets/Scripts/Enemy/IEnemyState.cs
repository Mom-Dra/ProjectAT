using UnityEngine;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using System;
using System.Runtime.Serialization;
using UnityEngine.AI;
using UnityEditor.Searcher;

public interface IEnemyState
{
    static readonly IEnemyState IdleState = new EnemyIdleState();
    static readonly IEnemyState PatrolState = new EnemyPatrolState();
    static readonly IEnemyState AttackState = new EnemyAttackState();
    static readonly IEnemyState ChaseState = new EnemyChaseState();
    static readonly IEnemyState SearchState = new EnemySearchState();
    static readonly IEnemyState CoverState = new EnemyCoverState();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}

public class EnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        //ColorDebug.RedLog("EnemyIdleState Enter");
        enemy.EnableFieldOfView(true);
        enemy.SetStateText("Idle");
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
        //ColorDebug.RedLog("EnemyPatrolState Enter");
        enemy.EnableFieldOfView(true);

        if (enemy.PatrolWaypoints is null || enemy.PatrolWaypoints.Count == 0)
        {
            Debug.LogWarning(enemy.name + "에게 순찰 경로가 없습니다.");
            enemy.ChangeState(IEnemyState.IdleState);
        }

        enemy.SetStateText("Patrol");
    }

    public void Update(Enemy enemy)
    {
        //ColorDebug.RedLog("Patrol Update");

        // 캡슐화
        if (enemy.IsAgentArrived())
        {
            enemy.CurrentWaypointIndex = (enemy.CurrentWaypointIndex + 1) % enemy.PatrolWaypoints.Count;
            enemy.SetDestinationOnAgent(enemy.PatrolWaypoints[enemy.CurrentWaypointIndex].position);
        }
    }

    public void Exit(Enemy enemy)
    {
        enemy.NavMeshAgent.ResetPath();
    }
}

public class EnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        //ColorDebug.RedLog("EnemyChaseState Enter");
        enemy.SetAttackMode(true);
        enemy.ResetTargetLostTimer();

        enemy.SetStateText("Chase");
    }

    public void Update(Enemy enemy)
    {
        //ColorDebug.RedLog("EnemyChaseState Update");
        if (enemy.IsTargetExist())
        {
            HandleTargetTracking(enemy);
        }
        else
        {
            HandleTargetLost(enemy);
        }
    }

    public void Exit(Enemy enemy)
    {
        enemy.SetAttackMode(false);
        enemy.ResetTargetLostTimer();
    }

    private void HandleTargetTracking(Enemy enemy)
    {
        enemy.StartInformTargetPositionCoroutine();
        enemy.ResetTargetLostTimer();

        if(!enemy.IsTargetInAttackRange())
        {
            ColorDebug.RedLog("Chase!!");
            enemy.Chase();

            return;
        }

        if (enemy.TryFindCover(out CoverPoint bestCover))
        {
            enemy.ChangeState(IEnemyState.CoverState);
        }
        else
        {
            enemy.ChangeState(IEnemyState.AttackState);
        }
    }

    private void HandleTargetLost(Enemy enemy)
    {
        ColorDebug.BlueLog("StopInformTargetPositionCoroutine");
        enemy.StopInformTargetPositionCoroutine();
        enemy.UpdateTargetLostTimer(Time.deltaTime);

        if (enemy.IsOverTargetLost())
            enemy.ChangeState(IEnemyState.SearchState);
    }
}

public class EnemyAttackState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        //ColorDebug.RedLog("EnemyAttackState Enter");
        enemy.SetAttackMode(true);
        enemy.SetStateText("Attack");
    }

    public void Update(Enemy enemy)
    {
        //ColorDebug.RedLog("AttackState Update");
        if(!enemy.IsTargetExist())
        {
            enemy.StopInformTargetPositionCoroutine();
            enemy.ChangeState(IEnemyState.ChaseState);
            return;
        }

        enemy.StartInformTargetPositionCoroutine();
        enemy.ResetTargetLostTimer();

        if (!enemy.IsTargetInAttackRange())
        {
            enemy.SetIdleAnimation();
            enemy.ChangeState(IEnemyState.ChaseState);
            return;
        }

        if(enemy.IsReloading())
        {
            enemy.SetIdleAnimation();
            return;
        }

        if (enemy.RotateTowardTarget())
        {
            enemy.Attack();
            enemy.SetAttackAnimation();
        }
    }

    public void Exit(Enemy enemy)
    {
        enemy.SetAttackMode(false);
    }
}

public class EnemySearchState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        enemy.SetAttackMode(true);
        //ColorDebug.RedLog("EnemySearchState Enter");

        enemy.SetStateText("Search");
    }

    public void Update(Enemy enemy)
    {
        //ColorDebug.RedLog("EnemySearchState Update");

        if (enemy.IsTargetExist())
        {
            enemy.ChangeState(IEnemyState.ChaseState);
            return;
        }
        
        if (enemy.IsAgentArrived())
        {
            PickNewSearchPoint(enemy);
        }
    }

    public void Exit(Enemy enemy)
    {
        enemy.SetAttackMode(false);
    }

    private void PickNewSearchPoint(Enemy enemy)
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * enemy.EnemyData.SearchRadius;
        Vector3 randomPoint = enemy.TargetLastKnownPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, enemy.EnemyData.SearchRadius, NavMesh.AllAreas))
        {
            enemy.SetDestinationOnAgent(hit.position);
        }
        else
        {
            enemy.SetDestinationOnAgent(enemy.TargetLastKnownPosition);
        }
    }
}

public class EnemyCoverState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        //ColorDebug.RedLog("EnemyCoverState Enter");
        enemy.SetAttackMode(true);

        enemy.SetStateText("Cover");

        if (enemy.ReservedCoverPoint is null)
        {
            enemy.ChangeState(IEnemyState.ChaseState);
        }
        else
        {
            ChangeSubState(enemy, ICoverSubState.MoveState);
        }
    }

    public void Update(Enemy enemy)
    {
        //if (!enemy.IsTargetExist())
        //{
        //    // 잠재적 문제 가능성
        //    // 공격해야 하는데 다시 커버도 해야하고 왔다 갔다 애매할 지도?
        //    enemy.ChangeState(IEnemyState.ChaseState);
        //    return;
        //}

        enemy.CurrCoverSubState.Update(enemy, this);
    }

    public void Exit(Enemy enemy)
    {
        enemy.CurrCoverSubState?.Exit(enemy, this);
        enemy.SetAttackMode(false);

        enemy.ReleaseCover();
    }

    internal void ChangeSubState(Enemy enemy, ICoverSubState coverSubState)
    {
        //if(enemy.CurrCoverSubState is not null)
        //{
        //    if (enemy.CurrCoverSubState == coverSubState) return;

        //    enemy.CurrCoverSubState?.Exit(enemy, this);
        //}

        enemy.CurrCoverSubState?.Exit(enemy, this);
        enemy.CurrCoverSubState = coverSubState;
        enemy.CurrCoverSubState.Enter(enemy, this);
    }
}

public interface ICoverSubState
{
    static readonly ICoverSubState MoveState = new EnemyCoverMoveState();
    static readonly ICoverSubState HideState = new EnemyCoverHideState();
    static readonly ICoverSubState PeekState = new EnemyCoverPeekState();
    static readonly ICoverSubState AttackState = new EnemyCoverAttackState();

    void Enter(Enemy enemy, EnemyCoverState enemyCoverState);
    void Update(Enemy enemy, EnemyCoverState enemyCoverState);
    void Exit(Enemy enemy, EnemyCoverState enemyCoverState);
}

public class EnemyCoverMoveState : ICoverSubState
{
    public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.SetStateText("CoverMove");
        enemy.SetDestinationOnAgent(enemy.ReservedCoverPoint.transform.position);
    }

    public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        if(enemy.IsAgentArrived())
        {
            enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
        }
    }

    public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
    {

    }
}

public class EnemyCoverHideState : ICoverSubState
{
    public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.SetStateText("CoverHide");
        enemy.ResetHideTimer();
        enemy.SetIsCrouch(true);
    }

    public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.UpdateHideTimer(Time.deltaTime);

        if(enemy.IsHideCompleted())
        {
            if(!enemy.IsTargetExist())
            {
                enemy.ChangeState(IEnemyState.ChaseState);
            }

            enemyCoverState.ChangeSubState(enemy, ICoverSubState.PeekState);
        }
    }

    public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.SetIsCrouch(false);
    }
}

public class EnemyCoverPeekState : ICoverSubState
{
    public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.SetStateText("CoverPeek");
        enemy.ResetPeekTimer();
    }

    public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.UpdateHideTimer(Time.deltaTime);

        if (!enemy.IsTargetExist())
        {
            enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
            return;
        }

        if (enemy.IsTargetInAttackRange())
        {
            enemyCoverState.ChangeSubState(enemy, ICoverSubState.AttackState);
        }
    }

    public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
    {

    }
}

public class EnemyCoverAttackState : ICoverSubState
{
    public void Enter(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        enemy.SetStateText("CoverAttack");
    }

    public void Update(Enemy enemy, EnemyCoverState enemyCoverState)
    {
        if (!enemy.IsTargetExist())
        {
            enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
            return;
        }

        enemy.StartInformTargetPositionCoroutine();
        enemy.ResetTargetLostTimer();

        if (!enemy.IsTargetInAttackRange())
            return;

        if(enemy.IsReloading())
        {
            enemy.SetIdleAnimation();
            enemyCoverState.ChangeSubState(enemy, ICoverSubState.HideState);
            return;
        }

        bool rotationComplete = enemy.RotateTowardTarget();

        if (!rotationComplete)
        {
            enemy.SetIdleAnimation();
            return;
        }

        enemy.Attack();
        enemy.SetAttackAnimation();
    }

    public void Exit(Enemy enemy, EnemyCoverState enemyCoverState)
    {

    }
}