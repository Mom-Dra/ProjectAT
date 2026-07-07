using UnityEngine;
using SkillOptionInterfaces;

//이 클래스는 삭제할 예정
public abstract class ConsumableSkill : Skill, IInventoryCostSkill
{
    protected Inventory entityInventory = null;
    protected ItemData neededItemData = null;
    protected int neededItemAmount = 0;

    protected ConsumableSkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
    {
        entityInventory = context.MyInventory;
        // if(skillData is ConsumableSkillData consumableData)
        // {
        //     neededItemData = consumableData.NeededItemData;
        //     neededItemAmount = consumableData.NeededItemAmount;   
        // }
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
