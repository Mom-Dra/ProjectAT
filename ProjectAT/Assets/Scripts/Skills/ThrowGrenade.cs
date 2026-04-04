
using UnityEngine;

public class ThrowGrenade : Skill
{
    private PlayerCombatModule myCombatModule;
    private ProjectileSkillData projSkillData => skillData as ProjectileSkillData;

    public ThrowGrenade(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        myCombatModule = context.MyCombatModule;
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
        Debug.Log($"Checking Extra : {context.CastedPosition}");
        return myCombatModule.CanThrowSomethingToPosition(projSkillData.ThrowingObjectPrefab, context.CastedPosition);
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        Debug.LogWarning("ThrowGrenade CanExecute is called. This should be replaced with actual logic to determine if the grenade can be thrown to the target position.");
        return true;
        //return context.MyCombatModule.CanThrowSomethingToPosition(targetPosition);
    }

    public override void Execute(SkillContext skillContext)
    {
        if(skillData is ProjectileSkillData projectileData)
        {
            GameObject grenade = UnityEngine.Object.Instantiate(projectileData.ThrowingObjectPrefab, skillContext.CastedPosition, Quaternion.identity);
            ProjectileGrenade proj = grenade.GetComponent<ProjectileGrenade>();
            proj.SetUp(projectileData.BaseDamage, projectileData.ExplosionRadius, projectileData.FuseTime, TargetLayer);
            context.MyCombatModule.ThrowSomthingToTarget(grenade, skillContext.CastedPosition);
        }
        else
        {
            Debug.LogWarning("SkillData for ThrowGrenade is not of type ProjectileSkillData. Please check the assigned SkillData.");
        }
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        Debug.Log($"{hit.collider.name} was hit. Checking if it can be selected as target for Throw Grenade...");
        if(((1 << hit.collider.gameObject.layer) & TargetLayer.value) != 0)
        {
            target = null;
            point = hit.point;
            return true;
        }
        
        target = null;
        point = Vector3.zero;
        Debug.Log("Invalid Target for Throw Grenade");
        return false;
    }
}
