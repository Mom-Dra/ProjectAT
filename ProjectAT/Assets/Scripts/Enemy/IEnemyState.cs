using UnityEngine;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using System;
using System.Runtime.Serialization;
using UnityEngine.AI;

public interface IEnemyState
{
    static readonly IEnemyState IdleState = new EnemyIdleState();
    static readonly IEnemyState PatrolState = new EnemyPatrolState();
    static readonly IEnemyState AttackState = new EnemyAttackState();
    static readonly IEnemyState ChaseState = new EnemyChaseState();
    static readonly IEnemyState SearchState = new EnemySearchState();

    void Enter(Enemy enemy);
    void Update(Enemy enemy);
    void Exit(Enemy enemy);
}

public class EnemyIdleState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        ColorDebug.RedLog("EnemyIdleState Enter");
        enemy.EnableFieldOfView(true);
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
        ColorDebug.RedLog("EnemyPatrolState Enter");
        enemy.EnableFieldOfView(true);

        if (enemy.PatrolWaypoints is null || enemy.PatrolWaypoints.Count == 0)
        {
            Debug.LogWarning(enemy.name + "¿¡°Ô ¼øÂû °æ·Î°¡ ¾ø½À´Ï´Ù.");
            enemy.ChangeState(IEnemyState.IdleState);
        }
    }

    public void Update(Enemy enemy)
    {
        ColorDebug.RedLog("Patrol Update");

        // Ä¸½¶È­
        if (!enemy.NavMeshAgent.pathPending && enemy.NavMeshAgent.remainingDistance <= enemy.NavMeshAgent.stoppingDistance)
        {
            enemy.CurrentWaypointIndex = (enemy.CurrentWaypointIndex + 1) % enemy.PatrolWaypoints.Count;
            enemy.NavMeshAgent.SetDestination(enemy.PatrolWaypoints[enemy.CurrentWaypointIndex].position);
        }
    }

    public void Exit(Enemy enemy)
    {
        //enemy.NavMeshAgent.ResetPath();
    }
}

public class EnemyChaseState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        ColorDebug.RedLog("EnemyChaseState Enter");

        enemy.SetAttackMode(true);
        enemy.ResetTargetLostTimer();
    }

    public void Update(Enemy enemy)
    {
        ColorDebug.RedLog("EnemyChaseState Update");

        if (enemy.IsTargetExist())
        {
            enemy.StartInformTargetPositionCoroutine();
            enemy.ResetTargetLostTimer();

            if (enemy.IsTargetInAttackRange()) enemy.ChangeState(IEnemyState.AttackState);
            else
            {
                ColorDebug.RedLog("Chase!!");
                enemy.Chase();
            }
        }
        else
        {
            ColorDebug.BlueLog("StopInformTargetPositionCoroutine");
            enemy.StopInformTargetPositionCoroutine();
            enemy.UpdateTargetLostTimer(Time.deltaTime);

            if (enemy.IsOverTargetLost())
            {
                enemy.ChangeState(IEnemyState.SearchState);
            }
        }
    }

    public void Exit(Enemy enemy)
    {
        enemy.SetAttackMode(false);
        enemy.ResetTargetLostTimer();
    }
}

public class EnemyAttackState : IEnemyState
{
    public void Enter(Enemy enemy)
    {
        ColorDebug.RedLog("EnemyAttackState Enter");
        enemy.SetAttackMode(true);
    }

    public void Update(Enemy enemy)
    {
        ColorDebug.RedLog("AttackState Update");

        if (enemy.IsTargetExist())
        {
            enemy.StartInformTargetPositionCoroutine();
            enemy.ResetTargetLostTimer();

            if (enemy.IsTargetInAttackRange())
            {
                bool rotationComplete = enemy.RotateTowardTarget();

                if (rotationComplete)
                    enemy.Attack();
            }
            else
            {
                enemy.ChangeState(IEnemyState.ChaseState);
            }
        }
        else
        {
            enemy.StopInformTargetPositionCoroutine();
            enemy.ChangeState(IEnemyState.ChaseState);
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
        ColorDebug.RedLog("EnemySearchState Enter");
    }

    public void Update(Enemy enemy)
    {
        ColorDebug.RedLog("EnemySearchState Update");

        if (enemy.IsTargetExist())
        {
            enemy.ChangeState(IEnemyState.ChaseState);
            return;
        }
        
        // Ä¸½¶È­
        if (!enemy.NavMeshAgent.pathPending && enemy.NavMeshAgent.remainingDistance <= enemy.NavMeshAgent.stoppingDistance)
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
