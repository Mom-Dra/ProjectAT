using UnityEngine;

public class DummySkill : Skill
{
    public DummySkill(PlayerSkillModule context, SkillData skillData) : base(context, skillData)
    {
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        return false;
    }

    public override void Execute(SkillContext skillContext)
    {
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        Debug.LogWarning("DummySkill IsValidTarget is called. This should be replaced with actual logic to determine if the target is valid for this skill.");
        target = null;
        point = Vector3.zero;
        return false;
    }
}
