using UnityEngine;

public class DummySkill : Skill
{
    public DummySkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
    {
    }
    
    public override Vector3 TargetPosition => Vector3.zero;


    public override void CancelSkill()
    {
        
    }

    public override void Execute()
    {
       Debug.Log("Dummy Skill Executed!");
    }

    public override bool CanExecute()
    {
        return true;
    }

    public override bool CanSelectTarget(in RaycastHit hit)
    {
        return true;
    }

    public override void OnCastingEnd()
    {
        
    }

    public override void OnCastingStart()
    {
    }

    public override void OnChasing()
    {
    }

    public override void OnChasingStart()
    {
    }

    public override void OnUiActivate()
    {
        Cursor.SetCursor(skillData.SkillIcon.texture, Vector2.zero, CursorMode.Auto);
    }

    public override void OnUiDeactivate()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public override void OnUiUpdate()
    {
    }
}
