using UnityEngine;
using Interactable;
using SkillOptionInterfaces;


public class UseBandage : TargetSkill, IInventoryCostSkill
{
    public UseBandage(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        entityInventory = context.MyInventory;
    }
    private EntityStatus targetStatus = null;
    private Inventory entityInventory = null;
    private ItemCostSkillData itemCostData => skillData as ItemCostSkillData;

    public ItemData NeededItemData => itemCostData?.NeededItemData;
    public int NeededItemAmount => itemCostData?.NeededItemAmount ?? 0;

    public override bool CanActivate()
    {
        return base.CanActivate () && HasEnoughItem();
    }

    public bool HasEnoughItem()
    {
        if(NeededItemData == null)
        {
            Debug.LogWarning($"{GetType().Name} requires an item but NeededItemData is null.");
            return false;
        }

        return entityInventory.GetItemCount(NeededItemData) >= NeededItemAmount;
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        if (targetStatus == null) return false;
        if (!HasEnoughItem()) return false;

        float range = CalculateFinalRange();
        return (targetStatus.transform.position - context.transform.position).sqrMagnitude <= 3.0f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        context.MyAnimModule.WeaponMeshVisible(false);
    }

    public override void Execute(SkillContext skillContext)
    {
        if (targetStatus == null) return;

        if(TryConsumeItem())
        {
            targetStatus.Heal(skillData.BaseDamage);
        }
    }

    public bool TryConsumeItem()
    {
        return entityInventory.TryUseItem(NeededItemData, NeededItemAmount);
    }

    public override void OnCastingEnd(SkillContext skillContext)
    {
        context.MyAnimModule.WeaponMeshVisible(true);
    }

    public override bool ExtraCastingCondition(SkillContext context)
    {
        return true;
    }

    public override float CalCulateFinalDamage()
    {
        return skillData.BaseDamage; //힐량으로 사용됨. 뭣하면 붕대 아이템의 스탯에 따라서
    }

    public override float CalculateFinalRange()
    {
        return 1.732f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    private bool CheckHealAvailable(in RaycastHit hit, out EntityStatus status)
    {
        status = null;
        return ((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0
         && hit.collider.gameObject.TryGetComponent(out status)
         && status.CurrentHp < status.MaxHp;
    }

    protected override bool CheckExtraConditionOnTarget(RaycastHit hit, out Collider castedCollider, out Vector3 point)
    {
        castedCollider = null;
        point = Vector3.zero;

        if(CheckHealAvailable(in hit, out EntityStatus status))
        {
            targetStatus = status;
            castedCollider = hit.collider;
            point = hit.point;
            return true;
        }
        
        point = Vector3.zero;
        Debug.Log("Bandage) Invalid Target.");
        return false;
    }
}
