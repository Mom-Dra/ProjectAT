using UnityEngine;

public abstract class TargetSkill : Skill
{
    protected TargetSkill(PlayerSkillModule context, SkillData skillData): base(context, skillData)
    {
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        target = null;
        point = Vector3.zero;

        if (hit.collider == null)
            return false;

        GameObject hitObject = hit.collider.gameObject;

        if (((1 << hitObject.layer) & TargetLayer.value) == 0)
            return false;

        return CheckExtraConditionOnTarget(hit, out target, out point);
    }

    protected abstract bool CheckExtraConditionOnTarget(
        RaycastHit hit,
        out GameObject target,
        out Vector3 point
    );
}
