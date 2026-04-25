using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

public class PlayerPerceptionModule : MonoBehaviour, IPerceivable
{
    private EntityStatus entityStatus;
    private IStealthable stealthable;

    public Transform Transform => transform;

    public bool IsValidTarget
    {
        get
        {
            if (!isActiveAndEnabled) return false;
            // if (stealthable.IsHidden) return false;
            if (entityStatus.IsDead) return false;

            return true;
        }
    }

    private void Awake()
    {
        entityStatus = GetComponent<EntityStatus>();
        // stealthable = GetComponent<IStealthable>();  e
    }
}
