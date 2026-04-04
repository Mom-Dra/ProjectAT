using System;
using System.Data.Common;
using UnityEngine;

public class ThrowGrenade : Skill
{
    public ThrowGrenade(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        
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
        }
        else
        {
            Debug.LogWarning("SkillData for ThrowGrenade is not of type ProjectileSkillData. Please check the assigned SkillData.");
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
        Debug.Log("Invalid Target for Throw Grenade");
        return false;
    }
}
