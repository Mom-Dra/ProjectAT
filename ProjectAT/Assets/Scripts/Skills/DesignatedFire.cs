using System.Collections;
using System.Data.Common;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DesignatedFire : Skill
{
    private PlayerCombatModule combatModule;
    public DesignatedFire(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        combatModule = context.MyCombatModule;
    }

    public override float CalCulateFinalDamage()
    {
        //return context.MyCombatModule.CalculateDamageWithWeapon(context.MyWeapon, skillData.BaseDamage);
        return skillData.BaseDamage;
    }

    public override float CalculateFinalRange()
    {
        return context.MyWeapon.Range;
    }

    public override bool ExtraCastingCondition(SkillContext context)
    {
        return combatModule.IsTargetInWeaponSight(context.TargetObject);
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        return context.MyCombatModule.IsTargetInWeaponSight(skillContext.TargetObject);
    }    
    
    public override void OnCastingStart(SkillContext skillContext)
    {
        context.MyAnimModule.SetAiming(true, skillContext.TargetObject?.transform);
    }

    public override void Execute(SkillContext skillContext)
    {
        if (skillContext.TargetObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(skillContext.FinalDamage);
            context.MyWeapon.FireWeapon();

            if (skillContext.TargetObject.TryGetComponent(out Enemy enemy) && context.TryGetComponent(out IPerceivable attacker))
                enemy.ReceiveAttack(attacker);
        }
    }

    public override void OnCastingEnd(SkillContext skillContext)
    {
        context.MyAnimModule.SetAiming(false, null);
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0 &&
           hit.collider.gameObject.TryGetComponent(out Enemy enemy)) //enemy를 굳이?
        {
            target = enemy.gameObject;
            point = enemy.transform.position;
            return true;
        }
        
        target = null;
        point = Vector3.zero;
        return false;
    }
}
