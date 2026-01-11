using UnityEngine;

public class UseBandage : Skill
{
    public UseBandage(PlayerSkillModule context, SkillData data) : base(context, data){}
    private EntityStatus targetStatus;

    public override Vector3 TargetPosition {get { return targetStatus? targetStatus.transform.position : Vector3.zero;}}


    public override void CancelSkill()
    {
        targetStatus = null;
    }

    public override bool CanExecute()
    {
        Debug.Log($"Bandage : {(targetStatus.transform.position - context.transform.position).sqrMagnitude}");
        return (targetStatus.transform.position - context.transform.position).sqrMagnitude <= 3.0f; //하드코딩됨. 플레이어의 hand 반경을 나타내는 값으로 교체 필요
    }

    public override bool CanSelectTarget(in RaycastHit hit)
    {
        //붕대 갯수 체크하는 로직 추가해야함.

        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0
         && hit.collider.gameObject.TryGetComponent<EntityStatus>(out EntityStatus status)&&
            status.CurrentHp < status.MaxHp)
        {
            targetStatus = status;
            return true;
        }

        return false;
    }

    public override void Execute()
    {
        targetStatus.Heal(skillData.Damage);
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
        throw new System.NotImplementedException();
    }
}
