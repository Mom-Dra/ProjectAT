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
        throw new System.NotImplementedException();
    }
}
