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
        //Cursor.SetCursor(skillData.CursorSkin, new Vector2(skillData.CursorSkin.width * 0.5f, skillData.CursorSkin.height * 0.5f), CursorMode.Auto);
        float indicatorSize = skillData.SkillEffectPrefab.GetComponent<ProjectileGrenade>().ExplosionRadius;
        context.MyEffectModule.ShowIndicator(targetPosition, IndicatorType.GroundSkillIndicator, indicatorSize);
    }

    public override void OnUiDeactivate()
    {
        //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        context.MyEffectModule.ClearThrowingLine();
        context.MyEffectModule.HideIndicator(IndicatorType.GroundSkillIndicator);
    }

    public override void OnUiUpdate()
    {
        //throw new System.NotImplementedException();
        if(context.MyPlayerController.RaycastAtMouseLocation(out RaycastHit hit))
        {
            float rad = context.MyStatus.ThrowRange;
            //context.MyEffectModule.DrawLineIndicator(context.MyPlayerController.transform.position, hit.point, IndicatorType.GroundSkillIndicator);
            if(rad * rad >= Vector3.SqrMagnitude(hit.point - context.transform.position))
            {
                context.MyEffectModule.DrawThrowingLine(hit.point, 2.0f);
            }
            else
            {
                context.MyEffectModule.ClearThrowingLine();
            }
            context.MyEffectModule.UpdateIndicator(hit.point, Vector3.zero, IndicatorType.GroundSkillIndicator);
        }
    }

    public override void CancelSkill()
    {
        targetPosition = Vector3.zero;
    }
}
