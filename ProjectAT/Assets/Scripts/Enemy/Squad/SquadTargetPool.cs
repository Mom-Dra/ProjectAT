using System.Collections.Generic;

public class SquadTargetPool
{
    private readonly Dictionary<IPerceivable, HashSet<ISquadMember>> watchers = new();

    public int TargetCount => watchers.Count;
    public bool HasAnyTarget => watchers.Count > 0;

    public IReadOnlyDictionary<IPerceivable, HashSet<ISquadMember>> Watchers => watchers;

    public void Add(ISquadMember reporter, IPerceivable target)
    {
        if (!watchers.TryGetValue(target, out HashSet<ISquadMember> set))
        {
            set = new HashSet<ISquadMember>();
            watchers[target] = set;
        }

        set.Add(reporter);
    }

    public bool Remove(ISquadMember reporter, IPerceivable target)
    {
        if (!watchers.TryGetValue(target, out HashSet<ISquadMember> set)) return false;
        set.Remove(reporter);

        if (set.Count == 0)
        {
            watchers.Remove(target);
            return true;
        }

        return false;
    }

    public void RemoveMemberFromAll(ISquadMember member, List<IPerceivable> removedTargets)
    {
        removedTargets.Clear();

        List<IPerceivable> toClean = new List<IPerceivable>();

        foreach (var kv in watchers)
        {
            if (kv.Value.Remove(member) && kv.Value.Count == 0)
                toClean.Add(kv.Key);
        }

        foreach (var t in toClean)
        {
            watchers.Remove(t);
            removedTargets.Add(t);
        }
    }

    public IPerceivable PickBestTarget()
    {
        IPerceivable best = null;
        int bestCount = 0;

        foreach (var kv in watchers)
        {
            if (!kv.Key.IsValidTarget) continue;

            if (kv.Value.Count > bestCount)
            {
                bestCount = kv.Value.Count;
                best = kv.Key;
            }
        }

        return best;
    }

    public void Clear() => watchers.Clear();
}
