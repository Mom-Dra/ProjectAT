using UnityEngine;

public class ThrowGrenade : Skill
{
    public ThrowGrenade(PlayerSkillModule context, SkillData data) : base(context, data){}


    public override bool CanExecute(Enemy target)
    {
        return context.MyCombatModule.CanThrowSomethingToEnemy(target);
    }

    public override void Execute(Enemy target)
    {
        context.MyCombatModule.ThrowSomthingToTarget(skillData.SkillEffectPrefab, target.transform.position);
        CurrSkillTime = Time.time;
    }

    public override void OnChasing(Enemy target)
    {
        context.MyMovementModule.PlayerWalk(target.transform.position);
    }

    public override void OnUiActivate()
    {
        Cursor.SetCursor(skillData.CursorSkin, Vector2.zero, CursorMode.Auto);
    }

    public override void OnUiDeactivate()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public override void OnUiUpdate()
    {
        //throw new System.NotImplementedException();
    }
}
