using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    [field: SerializeField]
    public string buffName { get; private set; }

    [field: SerializeField]
    public float Duration { get; private set; }

    [field: SerializeField]
    public float TickInterval { get; private set; } = 1.0f;

    public abstract void OnApply(GameObject target, BuffInstance buffInstance);
    public abstract void OnUpdate(GameObject target, BuffInstance buffInstance);
    public abstract void OnRemove(GameObject target, BuffInstance buffInstance);
}
