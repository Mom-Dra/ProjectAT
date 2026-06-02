using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;

public interface ISquadState
{
    static readonly ISquadState PatrolState = new SquadPatorlState();
    static readonly ISquadState EngageState = new SquadEngageState();
    static readonly ISquadState SearchState = new SquadSearchState();

    void Enter(Squad squad);
    void Update(Squad squad);
    void Exit(Squad squad);

    void TargetDetectedByMember(Squad squad, ISquadMember reporter, IPerceivable target) { }
    void TargetLostByMember(Squad squad, ISquadMember reporter, Vector3 lastKnownPosition, bool targetFullyLost) { }
    void TargetPositionUpdatedByMember(Squad squad, ISquadMember reporter, Vector3 lastKnownPosition) { }
    void MemberRemoved(Squad squad, ISquadMember squadMember) { }
}

public class SquadPatorlState : ISquadState
{
    public void Enter(Squad squad)
    {
        //squad.squadStateText.text = "Patrol";
        // AssignNext(squad);
    }

    public void Update(Squad squad)
    {
        // if (squad.Waypoints.Length == 0) return;

        // foreach (ISquadMember squadMember in squad.AliveMemgers)
        // {
        //     if (squadMember is Enemy enemy && !enemy.HasArrived()) return;
        // }

        // AssignNext(squad);
    }

    public void Exit(Squad squad)
    {

    }

    public void TargetDetectedByMember(Squad squad, ISquadMember reporter, IPerceivable target)
    {
        squad.ChangeState(ISquadState.EngageState);
    }

    private static void AssignNext(Squad squad)
    {
        if (squad.Waypoints.Length == 0) return;

        Vector3 waypoint = squad.Waypoints[squad.WaypointIndex].position;
        squad.WaypointIndex = (squad.WaypointIndex + 1) % squad.Waypoints.Length;

        foreach (ISquadMember squadMember in squad.AliveMembers)
        {
            Vector2 rnd = Random.insideUnitCircle * 1.5f;
            Vector3 t = waypoint + new Vector3(rnd.x, 0f, rnd.y);

            if (NavMesh.SamplePosition(t, out NavMeshHit hit, squad.SquadConfig.navSampleRadius, NavMesh.AllAreas))
                squadMember.ReceiveOrder(SquadOrder.Patrol(hit.position));
            else squadMember.ReceiveOrder(SquadOrder.Patrol(waypoint));
        }
    }
}

public class SquadEngageState : ISquadState
{
    public void Enter(Squad squad)
    {
        //squad.squadStateText.text = "Engage";
        squad.SlotTimer = 0f;
        squad.ReassignRequested = false;

        Reassign(squad);
    }

    public void Update(Squad squad)
    {
        squad.SlotTimer += Time.deltaTime;

        if (squad.ReassignRequested || squad.SlotTimer >= squad.SquadConfig.formationUpdateInterval)
        {
            squad.ReassignRequested = false;
            squad.SlotTimer = 0f;

            Reassign(squad);
        }
    }

    public void Exit(Squad squad)
    {

    }

    public void TargetDetectedByMember(Squad squad, ISquadMember reporter, IPerceivable target)
    {
        squad.ReassignRequested = true;
    }

    public void TargetLostByMember(Squad squad, ISquadMember reporter, Vector3 lastKnownPosition, bool targetFullyLost)
    {
        if (targetFullyLost)
        {
            if (!squad.SquadTargetPool.HasAnyTarget)
            {
                squad.ChangeState(ISquadState.SearchState);
            }
            else
            {
                squad.ReassignRequested = true;
            }
        }
    }

    public void MemberRemoved(Squad squad, ISquadMember squadMember)
    {
        squad.ReassignRequested = true;

        if (!squad.SquadTargetPool.HasAnyTarget)
            squad.ChangeState(ISquadState.SearchState);
    }

    private static void Reassign(Squad squad)
    {
        IPerceivable primaryTarget = squad.SquadTargetPool.PickBestTarget();
        if (primaryTarget is null) return;

        Vector3 center = primaryTarget.Transform.position;

        List<ISquadMember> assignable = new List<ISquadMember>();
        foreach (ISquadMember squadMember in squad.AliveMembers)
        {
            if (!squadMember.IsInCombat || ReferenceEquals(squadMember.CurrentTarget, primaryTarget))
                assignable.Add(squadMember);
        }

        if (assignable.Count == 0) return;

        Vector3 forward = center - squad.transform.position;
        forward.y = 0f;
        forward = forward.sqrMagnitude < float.Epsilon ? Vector3.forward : forward.normalized;

        squad.Formation.CalculateSlots(center, forward, assignable.Count, squad.FormationSlots);
        for (int i = 0; i < assignable.Count; ++i)
        {
            squad.FormationSlots[i] = NavMesh.SamplePosition(squad.FormationSlots[i], out NavMeshHit hit, squad.SquadConfig.navSampleRadius, NavMesh.AllAreas) ? hit.position : squad.FormationSlots[i];
        }

        assignable.Sort((a, b) =>
        {
            float da = (a.Transform.position - center).sqrMagnitude;
            float db = (b.Transform.position - center).sqrMagnitude;
            return da.CompareTo(db);
        });

        bool[] used = new bool[squad.FormationSlots.Length];

        foreach (ISquadMember assignableMember in assignable)
        {
            int bestIndex = -1;
            float best = float.MaxValue;

            for (int i = 0; i < assignable.Count; ++i)
            {
                if (used[i]) continue;

                float d = (assignableMember.Transform.position - squad.FormationSlots[i]).sqrMagnitude;

                if (d < best)
                {
                    best = d;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0)
            {
                used[bestIndex] = true;
                assignableMember.ReceiveOrder(SquadOrder.Attack(primaryTarget, squad.FormationSlots[bestIndex]));
            }
        }
    }
}

public class SquadSearchState : ISquadState
{
    public void Enter(Squad squad)
    {
        //squad.squadStateText.text = "Search";
        squad.Elapsed = 0f;
        squad.ReassignTimer = 0f;
    }

    public void Update(Squad squad)
    {
        squad.Elapsed += Time.deltaTime;

        // if (squad.Elapsed >= squad.SquadConfig.searchDuration)
        // {
        //     squad.ChangeState(ISquadState.PatrolState);
        //     return;
        // }

        squad.ReassignTimer += Time.deltaTime;

        if (squad.ReassignTimer >= squad.SquadConfig.searchReassignInterval)
        {
            squad.ReassignTimer = 0f;

            // Squad의 primary target 주변으로 계속해서 탐색..!
            Assign(squad);
        }
    }

    public void Exit(Squad squad)
    {

    }

    public void TargetDetectedByMember(Squad squad, ISquadMember reporter, IPerceivable target)
    {
        squad.ChangeState(ISquadState.EngageState);
    }

    private static void Assign(Squad squad)
    {
        SearchPointGenerator.Generate(
            squad.LastKnownPosition,
            squad.SquadConfig.searchRadius,
            squad.AliveMemgerCount,
            squad.FormationSlots,
            squad.SquadConfig.navSampleRadius);

        int idx = 0;
        foreach (ISquadMember squadMember in squad.AliveMembers)
        {
            if (squadMember.IsInCombat) continue;

            squadMember.ReceiveOrder(SquadOrder.Search(squad.FormationSlots[idx]));
            ++idx;
        }
    }
}
