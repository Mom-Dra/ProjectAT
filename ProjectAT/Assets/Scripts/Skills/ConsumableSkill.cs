using UnityEngine;

public abstract class ConsumableSkill : Skill
{
    protected ConsumableSkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
    {
            entityInventory = context.MyInventory;
            neededItemData = (skillData as ConsumableSkillData).NeededItemData;
    }

    protected ItemData neededItemData = null;
    protected Inventory entityInventory = null;

    public ItemData NeededItemData => neededItemData;

}
