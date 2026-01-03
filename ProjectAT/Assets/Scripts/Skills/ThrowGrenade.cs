using System;
using System.Data.Common;
using UnityEngine;

public class ThrowGrenade : Skill
{
    public ThrowGrenade(PlayerSkillModule context, SkillData data) : base(context, data){}
    private Vector3 targetPosition;
    public override Vector3 TargetPosition {get {return targetPosition;}}

    public override bool CanSelectTarget(in RaycastHit hit)
    {
        targetPosition = hit.point;
        return true;
    }

    public override bool CanExecute()
    {
        return context.MyCombatModule.CanThrowSomethingToPosition(targetPosition);
    }

    public override void OnCastingStart()
    {
        //Casting Start Logic
        Debug.Log("Throw Grenade Casting Started!");
        context.MyAnimModule.PlayGrenadeThrow();
    }

    public override void Execute()
    {
        context.MyCombatModule.ThrowSomthingToTarget(skillData.SkillEffectPrefab, targetPosition);
    }

    public override void OnCastingEnd()
    {
        CurrSkillTime = Time.time;
        targetPosition = Vector3.zero;
        context.MyAnimModule.PlayIdle();
    }

    public override void OnChasingStart()
    {
        context.MyAnimModule.PlayIdle();
    }
    public override void OnChasing()
    {
        context.MyMovementModule.PlayerWalk(targetPosition);
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
        //throw new System.NotImplementedException();
    }

    public override void CancelSkill()
    {
        targetPosition = Vector3.zero;
    }
}
