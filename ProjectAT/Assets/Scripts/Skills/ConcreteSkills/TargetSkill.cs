using UnityEngine;

public abstract class TargetSkill : Skill
{
    protected TargetSkill(PlayerSkillModule context, SkillData skillData): base(context, skillData)
    {
    }

    public override bool IsValidTarget(RaycastHit hit, out Collider castedCollider, out Vector3 point)
    {
        castedCollider = null;
        point = Vector3.zero;

        if (hit.collider == null)
            return false;

        GameObject hitObject = hit.collider.gameObject;

        if (!hitObject.IsSameLayer(TargetLayer))
            return false;

        return CheckExtraConditionOnTarget(hit, out castedCollider, out point);
    }

    protected abstract bool CheckExtraConditionOnTarget(
        RaycastHit hit,
        out Collider castedCollider,
        out Vector3 point
    );
}
