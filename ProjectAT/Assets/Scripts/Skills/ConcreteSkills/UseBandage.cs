using System;
using UnityEngine;
using Interactable;

public class UseBandage : ConsumableSkill
{
    public UseBandage(PlayerSkillModule context, SkillData data) : base(context, data){ }
    private EntityStatus targetStatus;

    public override bool CanActivate()
    {
        return base.CanActivate () && HasEnoughItem();
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        return (targetStatus.transform.position - context.transform.position).sqrMagnitude <= 3.0f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        context.MyAnimModule.WeaponMeshVisible(false);
    }

    public override void Execute(SkillContext skillContext)
    {
        if(TryConsumeItem())
        {
            if(targetStatus.IsDead && targetStatus.GetComponent<DownedBody>().enabled)
            {
                targetStatus.Revive(skillData.BaseDamage/4); //하드코딩됨. 기획에 따라 부활시 체력 어케할지 결정.
            }
            else
            {
                targetStatus.Heal(skillData.BaseDamage);
            }
        }
    }

    public override void OnCastingEnd(SkillContext skillContext)
    {
        context.MyAnimModule.WeaponMeshVisible(false);
    }

    public override bool ExtraCastingCondition(SkillContext context)
    {
        return entityInventory.GetItemCount(neededItemData) > 0;
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        if(CheckHealAvailable(in hit, out EntityStatus status))
        {
            targetStatus = status;
            target = hit.collider.gameObject;
            point = hit.point;
            return true;
        }
        
        target = null;
        point = Vector3.zero;
        Debug.Log("Bandage) Invalid Target.");
        return false;
    }

    public override float CalCulateFinalDamage()
    {
        return skillData.BaseDamage; //힐량으로 사용됨. 뭣하면 붕대 아이템의 스탯에 따라서
    }

    public override float CalculateFinalRange()
    {
        return 0.5f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    private bool CheckHealAvailable(in RaycastHit hit, out EntityStatus status)
    {
        status = null;
        return ((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0
         && hit.collider.gameObject.TryGetComponent<EntityStatus>(out status)
         && status.CurrentHp < status.MaxHp;
    }
}
