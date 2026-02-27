using System.Collections;
using System.Data.Common;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DesignatedFire : Skill
{
    public DesignatedFire(PlayerSkillModule context, SkillData data) : base(context, data){}
    private Enemy targetEnemy;
    public override Vector3 TargetPosition {get { return targetEnemy? targetEnemy.transform.position : Vector3.zero;}}

    public override void OnChasingStart()
    {
        context.MyAnimModule.PlayIdle();
    }

    public override void OnChasing()
    {
        context.MyMovementModule.PlayerWalk(TargetPosition);
    }

    public override bool CanSelectTarget(in RaycastHit hit)
    {
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0 &&
           hit.collider.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
        {
            targetEnemy = enemy;
            return true;
        }

        return false;
    }

    public override bool CanExecute()
    {
        return context.MyCombatModule.IsEnemyInWeaponSight(targetEnemy);
    }

    public override void OnCastingStart()
    {
        //Casting Start Logic
        Debug.Log("Designated Fire Casting Started!");
        context.MyAnimModule.PlayAiming();
    }

    public override void Execute()
    {
        //Snping
        Debug.Log("Designated Fire Executed!");

        if (targetEnemy.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(skillData.Damage);
            context.MyWeapon.FireWeapon();
        }
    }

    public override void OnCastingEnd()
    {
        targetEnemy = null;
        CurrSkillTime = Time.time;
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
        //throw new System.NotImplementedException();
    }

    public override void CancelSkill()
    {
        targetEnemy = null;
    }
}
