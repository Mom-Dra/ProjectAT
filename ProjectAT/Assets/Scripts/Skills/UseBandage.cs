using UnityEngine;

public class UseBandage : ConsumableSkill
{
    public UseBandage(PlayerSkillModule context, SkillData data) : base(context, data){ }
    private EntityStatus targetStatus;
    public override Vector3 TargetPosition {get { return targetStatus? targetStatus.transform.position : Vector3.zero;}}

    public override void CancelSkill()
    {
        targetStatus = null;
    }

    public override bool CanExecute()
    {
        return (targetStatus.transform.position - context.transform.position).sqrMagnitude <= 3.0f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    public override bool CanSelectTarget(in RaycastHit hit)
    {
        //붕대 갯수 체크하는 로직 추가해야함.
        Debug.Log($"Use Bandage) {hit.collider.gameObject.name} was hit. Checking if it can be selected as target...");
        Debug.Log($"Use Bandage) Checking Target Layer... Target Layer: {1 << hit.collider.gameObject.layer}, Allowed Layer: {TargetLayer.value}");
        bool conditionOne = ((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0;
        if(!conditionOne)
        {
            Debug.Log("Bandage) Cannot Select Target. Invalid Target Layer.");
            return false;
        }
        bool conditionTwo = hit.collider.gameObject.TryGetComponent<EntityStatus>(out EntityStatus status);
        if(!conditionTwo)
        {
            Debug.Log("Bandage) Cannot Select Target. No EntityStatus Component Found.");
            return false;
        }
        bool conditionThree = status.CurrentHp < status.MaxHp;
        if(!conditionThree)
        {
            Debug.Log("Bandage) Cannot Select Target. Target HP is Full.");
            return false;
        }

        if(conditionOne && conditionTwo && conditionThree)
        {
            targetStatus = status;
            return true;
        }

        // if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0
        //  && hit.collider.gameObject.TryGetComponent<EntityStatus>(out EntityStatus status)&&
        //     status.CurrentHp < status.MaxHp)
        // {
        //     targetStatus = status;
        //     return true;
        // }
        return false;
    }

    public override void Execute()
    {
        if(entityInventory.TryUseItem(neededItemData, 1))
        {
            if(targetStatus.IsDead)
            {
                targetStatus.Revive(skillData.Damage/4); //하드코딩됨. 기획에 따라 부활시 체력 어케할지 결정.
            }
            else
            {
                targetStatus.Heal(skillData.Damage);
            }
            Debug.Log("Use Bandage Executed!");            
        }
        
    }

    public override void OnCastingEnd()
    {
        CurrSkillTime = Time.time;
        targetStatus = null;
        context.MyAnimModule.PlayIdle();
    }

    public override void OnCastingStart()
    {
        //Casting Start Logic
        Debug.Log("Use Bandage Casting Started!");
        context.MyAnimModule.PlayUseItem();
    }

    public override void OnChasing()
    {
        context.MyMovementModule.PlayerWalk(TargetPosition);
    }

    public override void OnChasingStart()
    {
        context.MyAnimModule.PlayIdle();
    }
    
    public override bool CanActivateSkill()
    {
        if (!base.CanActivateSkill())
        {
            Debug.Log("Cannot Activate Skill. Skill Cooltime Remaining.");
            return false;
        }
        
        if(entityInventory.GetItemCount(neededItemData) < 1)
        {
            Debug.Log("Cannot Activate Skill. Not Enough Items.");
            return false;
        }
        return true;
        //return base.CanActivateSkill() && entityInventory.GetItemCount(neededItemData) > 0;
    }

    public override void OnUiActivate()
    {
        Cursor.SetCursor(skillData.CursorSkin, new Vector2(skillData.CursorSkin.width * 0.5f, skillData.CursorSkin.height * 0.5f), CursorMode.Auto);
    }

    public override void OnUiDeactivate()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public override void OnUiUpdate()
    {
        //Do nothing
    }
}
