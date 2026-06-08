using UnityEngine;
using SkillOptionInterfaces;
using SkillDataOptionInterfaces;

public class DesignatedFire : TargetSkill, IWeaponUsingSkill
{
    private readonly PlayerCombatModule combatModule;
    private readonly PlayerAnimator animModule;
    private WeaponSkillData  weaponSkillData => skillData as WeaponSkillData ;

    public DesignatedFire(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        combatModule = context.MyCombatModule;
        animModule = context.MyAnimModule;
    }

    public bool RequiresAmmo => weaponSkillData?.RequiresAmmo?? true;

    public bool HasEnoughAmmo()
    {
        if(!RequiresAmmo) return true;
        else return context.MyWeapon != null && context.MyWeapon.HasAmmoInMagazine();
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && HasEnoughAmmo();
    }

    public override bool ExtraCastingCondition(SkillContext skillContext)
    {
        return CheckTargetActivation(skillContext) && combatModule.IsTargetInWeaponSight(skillContext.TargetObject);
    }

    private bool CheckTargetActivation(SkillContext skillContext)
    {
        return skillContext != null &&
               skillContext.TargetObject != null &&
               skillContext.TargetObject.activeInHierarchy;
    }

    protected override bool CheckExtraConditionOnTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        if (hit.collider.gameObject.TryGetComponent(out Enemy enemy))
        {
            target = enemy.gameObject;
            point = enemy.transform.position;
            return true;
        }

        target = null;
        point = Vector3.zero;
        return false;
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        return  CheckTargetActivation(skillContext) && 
                combatModule.IsTargetInWeaponSight(skillContext.TargetObject) && 
                context.MyWeapon.CanFire();
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        animModule.SetAiming(true, skillContext.TargetObject?.transform);
    }

    public override void Execute(SkillContext skillContext)
    {
        if (!CheckTargetActivation(skillContext)) return;
        if (!skillContext.TargetObject.TryGetComponent(out IDamageable damageable)) return;
        
        damageable.TakeDamage(skillContext.FinalDamage);
        context.MyWeapon.FireWeaponOnlyVFX(skillContext.CastedPosition, false); // 0 데미지로 발사 연출/탄약 소모/발사 이벤트만 처리.
    }

    public override void OnCastingEnd(SkillContext skillContext)
    {
        animModule.SetAiming(false, null);
    }

    public override float CalCulateFinalDamage()
    {
        return skillData.BaseDamage;
    }

    public override float CalculateFinalRange()
    {
        return context.MyWeapon.Range;
    }


    // public override float CalCulateFinalDamage()
    // {
    //     //return context.MyCombatModule.CalculateDamageWithWeapon(context.MyWeapon, skillData.BaseDamage);
    //     return skillData.BaseDamage;
    // }

    // public override float CalculateFinalRange()
    // {
    //     return context.MyWeapon.Range;
    // }

    // public override bool ExtraCastingCondition(SkillContext context)
    // {
    //     return combatModule.IsTargetInWeaponSight(context.TargetObject);
    // }

    // public override bool CanExecute(SkillContext skillContext)
    // {
    //     return combatModule.IsTargetInWeaponSight(skillContext.TargetObject) && context.MyWeapon.CanFire();
    // }    

    // public override void OnCastingStart(SkillContext skillContext)
    // {
    //     context.MyAnimModule.SetAiming(true, skillContext.TargetObject?.transform);
    // }

    // public override void Execute(SkillContext skillContext)
    // {
    //     if (skillContext.TargetObject.TryGetComponent(out IDamageable damageable))
    //     {
    //         damageable.TakeDamage(skillContext.FinalDamage);
    //         context.MyWeapon.FireWeapon(0.0f, TargetLayer.value); //눈속임을 위해 0데미지를 줌.
    //     }
    // }

    // public override void OnCastingEnd(SkillContext skillContext)
    // {
    //     context.MyAnimModule.SetAiming(false, null);
    // }

    // public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    // {
    //     if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0 &&
    //        hit.collider.gameObject.TryGetComponent(out Enemy enemy)) //enemy를 굳이?
    //     {
    //         target = enemy.gameObject;
    //         point = enemy.transform.position;
    //         return true;
    //     }

    //     target = null;
    //     point = Vector3.zero;
    //     return false;
    // }
}
