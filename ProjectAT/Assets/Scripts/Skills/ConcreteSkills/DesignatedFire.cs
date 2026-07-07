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
        return CheckTargetActivation(skillContext) && combatModule.IsTargetVisible(skillContext.TargetCollider, TargetLayer);
    }

    private bool CheckTargetActivation(SkillContext skillContext)
    {
        return skillContext != null &&
               skillContext.TargetCollider != null &&
               skillContext.TargetCollider.gameObject.activeInHierarchy;
    }

    protected override bool CheckExtraConditionOnTarget(RaycastHit hit, out Collider castedCollider, out Vector3 point)
    {
        castedCollider = null;

        if (hit.collider.gameObject.TryGetComponent<Enemy>(out _))
        {
            castedCollider = hit.collider;
            point = hit.collider.bounds.center;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        return  CheckTargetActivation(skillContext) && 
                combatModule.IsTargetInWeaponSight(skillContext.TargetCollider, TargetLayer) && 
                context.MyWeapon.CanFire();
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        animModule.SetAiming(true, skillContext.TargetCollider?.transform);
    }

    public override void Execute(SkillContext skillContext)
    {
        if (!CheckTargetActivation(skillContext)) return;
        if (!skillContext.TargetCollider.gameObject.TryGetComponent(out IDamageable damageable)) return;
        
        damageable.TakeDamage(skillContext.FinalDamage);
        context.MyWeapon.FireWeaponOnlyVFX(skillContext.CastedPosition, Vector3.up, false); // 0 데미지로 발사 연출/탄약 소모/발사 이벤트만 처리.
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
}
