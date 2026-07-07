using UnityEngine;

public class DummySkill : Skill
{
    public DummySkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
    {
    }

    public override float CalCulateFinalDamage()
    {
        Debug.LogWarning("DummySkill : CalCulateFinalDamage is called. This should be replaced with actual damage calculation logic.");
        return skillData.BaseDamage;
    }

    public override float CalculateFinalRange()
    {
        Debug.LogWarning("DummySkill : CalculateFinalRange is called. This should be replaced with actual range calculation logic.");
        return context.MyWeapon.Range;
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        Debug.LogWarning("DummySkill : CanExecute is called. This should be replaced with actual logic to determine if the skill can be executed in the current context.");
        return false;
    }

    public override void Execute(SkillContext skillContext)
    {
        Debug.LogWarning("DummySkill : Execute is called. This should be replaced with actual execution logic for the skill.");
    }

    public override bool IsValidTarget(RaycastHit hit, out Collider castedCollider, out Vector3 point)
    {
        Debug.LogWarning("DummySkill : IsValidTarget is called. This should be replaced with actual logic to determine if the target is valid for this skill.");
        castedCollider = null;
        point = Vector3.zero;
        return false;
    }
}
