using UnityEngine;

public interface IPerceivable
{
    Transform Transform { get; }
    bool IsValidTarget { get; }
}
