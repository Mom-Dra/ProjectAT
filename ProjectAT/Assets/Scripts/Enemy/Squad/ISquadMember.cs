using System;
using UnityEngine;

public interface ISquadMember
{
    event Action<ISquadMember, IPerceivable> onTargetDetected;
    event Action<ISquadMember, IPerceivable, Vector3> onTargetLost;
    event Action<ISquadMember, IPerceivable, Vector3> onTargetPositionUpdated;

    IPerceivable CurrentTarget { get; }
    bool IsEngaging { get; }
    bool IsAlive { get; }
    Transform Transform { get; }

    void ReceiveOrder(SquadOrder squadOrder);
}
