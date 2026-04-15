using UnityEngine;

public class UseBandage : ConsumableSkill
{
    public UseBandage(PlayerSkillModule context, SkillData data) : base(context, data){ }
    private EntityStatus targetStatus;

    public override bool CanExecute(SkillContext skillContext)
    {
        return (targetStatus.transform.position - context.transform.position).sqrMagnitude <= 3.0f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        //애니메이션 넣기
        context.MyWeapon.NowWeaponVisible(false);
    }

    public override void Execute(SkillContext skillContext)
    {
        if(entityInventory.TryUseItem(neededItemData, 1))
        {
            if(targetStatus.IsDead)
            {
                targetStatus.Revive(skillData.BaseDamage/4); //하드코딩됨. 기획에 따라 부활시 체력 어케할지 결정.
            }
            else
            {
                targetStatus.Heal(skillData.BaseDamage);
            }
            Debug.Log("Use Bandage Executed!");            
        }
    }

    public override void OnCastingEnd(SkillContext skillContext)
    {
        //애니메이션 넣기
        context.MyWeapon.NowWeaponVisible(true);
    }

    public override bool ExtraCastingCondition(SkillContext context)
    {
        return entityInventory.GetItemCount(neededItemData) > 0;
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0
         && hit.collider.gameObject.TryGetComponent<EntityStatus>(out EntityStatus status)&&
            status.CurrentHp < status.MaxHp)
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

    #region  old code
    // public override bool CanSelectTarget(in RaycastHit hit)
    // {
    //     //붕대 갯수 체크하는 로직 추가해야함.
    //     Debug.Log($"Use Bandage) {hit.collider.gameObject.name} was hit. Checking if it can be selected as target...");
    //     Debug.Log($"Use Bandage) Checking Target Layer... Target Layer: {1 << hit.collider.gameObject.layer}, Allowed Layer: {TargetLayer.value}");
    //     bool conditionOne = ((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0;
    //     if(!conditionOne)
    //     {
    //         Debug.Log("Bandage) Cannot Select Target. Invalid Target Layer.");
    //         return false;
    //     }
    //     bool conditionTwo = hit.collider.gameObject.TryGetComponent<EntityStatus>(out EntityStatus status);
    //     if(!conditionTwo)
    //     {
    //         Debug.Log("Bandage) Cannot Select Target. No EntityStatus Component Found.");
    //         return false;
    //     }
    //     bool conditionThree = status.CurrentHp < status.MaxHp;
    //     if(!conditionThree)
    //     {
    //         Debug.Log("Bandage) Cannot Select Target. Target HP is Full.");
    //         return false;
    //     }

    //     if(conditionOne && conditionTwo && conditionThree)
    //     {
    //         targetStatus = status;
    //         return true;
    //     }

    //     // if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0
    //     //  && hit.collider.gameObject.TryGetComponent<EntityStatus>(out EntityStatus status)&&
    //     //     status.CurrentHp < status.MaxHp)
    //     // {
    //     //     targetStatus = status;
    //     //     return true;
    //     // }
    //     return false;
    // }
    //     public override void OnCastingEnd()
    // {
    //     CurrSkillTime = Time.time;
    //     targetStatus = null;
    //     context.MyAnimModule.PlayIdle();
    // }

    // public override void OnCastingStart()
    // {
    //     //Casting Start Logic
    //     Debug.Log("Use Bandage Casting Started!");
    //     context.MyAnimModule.PlayUseItem();
    // }

    // public override void OnChasing()
    // {
    //     context.MyMovementModule.PlayerWalk(TargetPosition);
    // }

    // public override void OnChasingStart()
    // {
    //     context.MyAnimModule.PlayIdle();
    // }

    // public override bool CanActivateSkill()
    // {
    //     if (!base.CanActivateSkill())
    //     {
    //         Debug.Log("Cannot Activate Skill. Skill Cooltime Remaining.");
    //         return false;
    //     }

    //     if(entityInventory.GetItemCount(neededItemData) < 1)
    //     {
    //         Debug.Log("Cannot Activate Skill. Not Enough Items.");
    //         return false;
    //     }
    //     return true;
    //     //return base.CanActivateSkill() && entityInventory.GetItemCount(neededItemData) > 0;
    // }

    // public override void OnUiActivate()
    // {
    //     Cursor.SetCursor(skillData.CursorSkin, new Vector2(skillData.CursorSkin.width * 0.5f, skillData.CursorSkin.height * 0.5f), CursorMode.Auto);
    // }

    // public override void OnUiDeactivate()
    // {
    //     Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    // }

    // public override void OnUiUpdate()
    // {
    //     //Do nothing
    // }
    #endregion
}
