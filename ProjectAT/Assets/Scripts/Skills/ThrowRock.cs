using UnityEngine;

public class ThrowRock : Skill
{
    private PlayerCombatModule myCombatModule;
    private PlayerAnimator myAnimator;
    private ProjectileSkillData projSkillData => skillData as ProjectileSkillData;

    public ThrowRock(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        myCombatModule = context.MyCombatModule;
        myAnimator = context.MyAnimModule;
    }

    public override float CalCulateFinalDamage()
    {
        return skillData.BaseDamage;
    }

    public override float CalculateFinalRange()
    {
        return context.MyStatus.ThrowRange;
    }

    public override bool ExtraCastingCondition(SkillContext context)
    {
        return myCombatModule.CanThrowSomethingToPosition(projSkillData.ThrowingObjectPrefab, context.CastedPosition);
    }

    public override bool CanExecute(SkillContext context)
    {
        //return true;
        return myCombatModule.CanThrowSomethingToPosition(projSkillData.ThrowingObjectPrefab, context.CastedPosition);
    }

    public override void Execute(SkillContext skillContext)
    {
        if(skillData is ProjectileSkillData projectileData)
        {
            GameObject rock = Object.Instantiate(projectileData.ThrowingObjectPrefab, skillContext.CastedPosition, Quaternion.identity);
            context.MyCombatModule.ThrowSomthingToTarget(rock, skillContext.CastedPosition);
        }
        else
        {
            Debug.LogWarning("SkillData for ThrowRock is not of type ProjectileSkillData. Please check the assigned SkillData.");
        }
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0)
        {
            target = null;
            point = hit.point;
            return true;
        }
        
        target = null;
        point = Vector3.zero;
        return false;
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        //애니메이션
        //myCombatModule.MyWeapon.NowWeaponVisible(false);
        myAnimator.WeaponMeshVisible(false);
        context.MyAnimModule.PlayThrowAnimation();
    }

    public override void OnCastingEnd(SkillContext skillContext)
    {
        //애니메이션
        myAnimator.WeaponMeshVisible(true);
    }

}
