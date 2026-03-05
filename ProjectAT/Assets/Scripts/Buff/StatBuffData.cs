using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Buffs/StatBuff")]
public class StatBuffData : BuffData
{
    public StatType TargetStatType;
    public float Amount;
    public ModifierType ModifierType = ModifierType.PercentAdd;

    public override void OnApply(GameObject target, BuffInstance buffInstance)
    {
        if (target.TryGetComponent(out EntityStatus entityStatus))
        {
            Stat stat = entityStatus.GetStat(TargetStatType);

            if (stat is not null)
                stat.AddModifier(new StatModifier(Amount, ModifierType, buffInstance));
        }
    }

    public override void OnUpdate(GameObject target, BuffInstance buffInstance)
    {

    }

    public override void OnRemove(GameObject target, BuffInstance buffInstance)
    {
        if (target.TryGetComponent(out EntityStatus entityStatus))
        {
            Stat stat = entityStatus.GetStat(TargetStatType);

            if (stat is not null)
                stat.RemoveAllModifiersFromSource(buffInstance);
        }
    }
}