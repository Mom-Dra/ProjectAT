using System.Collections.Generic;
using UnityEngine;

public class TargetMemory
{
    public HashSet<ISquadMember> Watchers { get; } = new HashSet<ISquadMember>();
    public Vector3 LastKnownPosition { get; set; }
    public float LastUpdateTime { get; set; }
    public bool HasWatchers => Watchers.Count > 0;

    public TargetMemory(Vector3 initialPosition)
    {
        LastKnownPosition = initialPosition;
        LastUpdateTime = Time.time;
    }
}
