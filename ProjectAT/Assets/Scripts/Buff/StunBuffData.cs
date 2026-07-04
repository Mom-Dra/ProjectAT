using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Buffs/StunBuff")]
public class StunBuffData : BuffData
{
    public override void OnApply(GameObject target, BuffInstance buffInstance)
    {
        if (target.TryGetComponent(out CrowdControlModule crowdControlModule))
        {
            crowdControlModule.ApplyStun(buffInstance);
            return;
        }

        Debug.LogWarning($"{target.name} does not have a CrowdControlModule.", target);
    }

    public override void OnUpdate(GameObject target, BuffInstance buffInstance)
    {
    }

    public override void OnRemove(GameObject target, BuffInstance buffInstance)
    {
        if (target.TryGetComponent(out CrowdControlModule crowdControlModule))
        {
            crowdControlModule.RemoveStun(buffInstance);
        }
    }
}
