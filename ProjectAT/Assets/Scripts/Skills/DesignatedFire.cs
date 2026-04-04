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

    public override void Execute(SkillContext skillContext)
    {
        if (skillContext.TargetObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(skillContext.FinalDamage);
            context.MyWeapon.FireWeapon();
        }
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0 &&
           hit.collider.gameObject.TryGetComponent(out Enemy enemy))
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
