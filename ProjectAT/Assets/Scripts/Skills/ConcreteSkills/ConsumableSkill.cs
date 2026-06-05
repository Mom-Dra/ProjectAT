using UnityEngine;
using SkillOptionInterfaces;

public abstract class ConsumableSkill : Skill, IInventoryCostSkill
{
    protected Inventory entityInventory = null;
    protected ItemData neededItemData = null;
    protected int neededItemAmount = 0;

    protected ConsumableSkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
    {
        entityInventory = context.MyInventory;
        if(skillData is ConsumableSkillData consumableData)
        {
            neededItemData = consumableData.NeededItemData;
            neededItemAmount = consumableData.NeededItemAmount;   
        }
    }


    public ItemData NeededItemData => neededItemData;
    public int NeededItemAmount => neededItemAmount;


    public bool HasEnoughItem()
    {
        return neededItemData != null && entityInventory.GetItemCount(neededItemData) >= NeededItemAmount;
    }

    public bool TryConsumeItem()
    {
        return neededItemData != null && entityInventory.TryUseItem(neededItemData, NeededItemAmount);
    }
}
