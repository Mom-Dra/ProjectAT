using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Squad : MonoBehaviour
{
    [SerializeField] private SquadConfig squadConfig;
    [SerializeField] private Enemy[] initialMembers;
    [SerializeField] private Transform[] patrolWaypoints;

    [SerializeField] internal TextMeshProUGUI squadStateText;

    private readonly List<ISquadMember> members = new List<ISquadMember>();
    private readonly SquadTargetPool squadTargetPool = new SquadTargetPool();
    private readonly List<IPerceivable> scratchRemoved = new List<IPerceivable>();

    private IFormation formation;
    private ISquadState currState;
    private Vector3 lastKnownPosition;

    private int waypointIndex;


    // State
    internal SquadConfig SquadConfig => squadConfig;
    internal SquadTargetPool SquadTargetPool => squadTargetPool;
    internal Vector3 LastKnownPosition => lastKnownPosition;

    internal IEnumerable<ISquadMember> AliveMembers
    {
        get
        {
            foreach (ISquadMember member in members)
                if (member is not null && member.IsAlive) yield return member;
        }
    }

    internal int AliveMemgerCount
    {
        get
        {
            int count = 0;

            foreach (ISquadMember member in members)
                if (member is not null && member.IsAlive) ++count;

            return count;
        }
    }

    internal IFormation Formation => formation;
    internal Vector3[] FormationSlots { get; private set; }

    internal Transform[] Waypoints => patrolWaypoints;
    internal int WaypointIndex
    {
        get => waypointIndex;
        set
        {
            if (value < 0 || value >= patrolWaypoints.Length)
            {
                Debug.LogError("value < 0 || value >= patrolWaypoints.Length");
                return;
            }

            waypointIndex = value;
        }
    }

    internal float SlotTimer { get; set; }
    internal bool ReassignRequested { get; set; }

    internal float Elapsed { get; set; }
    internal float ReassignTimer { get; set; }

    private void Awake()
    {
        foreach (Enemy enemy in initialMembers)
        {
            AddMember(enemy);
        }

        formation = new CircleFormation(squadConfig.searchRadius);
        FormationSlots = new Vector3[initialMembers.Length];
    }

    private void Start()
    {
        ChangeState(ISquadState.PatrolState);
    }

    private void Update()
    {
        currState?.Update(this);
    }

    private void OnDestroy()
    {
        foreach (ISquadMember member in members)
        {
            UnsubscribeMember(member);
            ReleaseMember(member);
        }

        members.Clear();
        squadTargetPool.Clear();
    }

    public void AddMember(ISquadMember squadMember)
    {
        if (squadMember is null || members.Contains(squadMember))
        {
            Debug.LogError("squadMember is null || members already has member");
            return;
        }

        members.Add(squadMember);
        RegisterMember(squadMember);
        SubscribeMember(squadMember);
    }

    public void RemoveMember(ISquadMember squadMember)
    {
        if (squadMember is null)
        {
            Debug.LogError("squadMember is null");
            return;
        }

        UnsubscribeMember(squadMember);
        ReleaseMember(squadMember);

        members.Remove(squadMember);
        squadTargetPool.RemoveMemberFromAll(squadMember, scratchRemoved);
        currState?.MemberRemoved(this, squadMember);
    }

    private void RegisterMember(ISquadMember squadMember)
    {
        if (squadMember is Enemy enemy)
            enemy.JoinSquad(this);
    }

    private void ReleaseMember(ISquadMember squadMember)
    {
        if (squadMember is Enemy enemy)
            enemy.LeaveSquad(this);
    }

    private void SubscribeMember(ISquadMember squadMember)
    {
        squadMember.onTargetDetected += TargetDetectedByMember;
        squadMember.onTargetLost += TargetLostByMember;
        squadMember.onTargetPositionUpdated += TargetPositionUpdatedByMember;
    }

    private void UnsubscribeMember(ISquadMember squadMember)
    {
        squadMember.onTargetDetected -= TargetDetectedByMember;
        squadMember.onTargetLost -= TargetLostByMember;
        squadMember.onTargetPositionUpdated -= TargetPositionUpdatedByMember;
    }

    private void TargetDetectedByMember(ISquadMember reporter, IPerceivable target)
    {
        if (target is null || !target.IsValidTarget)
        {
            Debug.LogError("target is null || target is invalid");
            return;
        }

        squadTargetPool.Add(reporter, target);
        lastKnownPosition = target.Transform.position;

        currState?.TargetDetectedByMember(this, reporter, target);
    }

    private void TargetLostByMember(ISquadMember reporter, IPerceivable target, Vector3 lastKnownPosition)
    {
        if (target is null)
        {
            Debug.LogError("target is null");
            return;
        }

        bool targetFullyLost = squadTargetPool.Remove(reporter, target);
        this.lastKnownPosition = lastKnownPosition;

        currState?.TargetLostByMember(this, reporter, lastKnownPosition, targetFullyLost);
    }

    private void TargetPositionUpdatedByMember(ISquadMember reporter, IPerceivable target, Vector3 lastKnownPosition)
    {
        this.lastKnownPosition = lastKnownPosition;
        currState?.TargetPositionUpdatedByMember(this, reporter, lastKnownPosition);
    }

    internal void ChangeState(ISquadState nextState)
    {
        if (nextState is null)
        {
            Debug.LogError("nextState is null");
            return;
        }

        if (ReferenceEquals(currState, nextState)) return;

        currState?.Exit(this);
        currState = nextState;
        currState.Enter(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(lastKnownPosition, 1f);
    }
#endif
}
