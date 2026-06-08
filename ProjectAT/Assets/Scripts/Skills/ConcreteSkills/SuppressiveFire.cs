using System.Collections.Generic;
using SkillOptionInterfaces;
using UnityEngine;

public class SuppressiveFire : Skill, IWeaponUsingSkill
{
    private readonly PlayerCombatModule combatModule;
    private readonly PlayerAnimator animator;

    private readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();
    private readonly Collider[] hits = new Collider[32];
    

    private DurationAreaWeaponSkillData suppressiveData => skillData as DurationAreaWeaponSkillData;

    private float elapsed;
    private float tickTimer;

    private float AreaWidth => suppressiveData != null ? suppressiveData.AoERadius : 0f;

    public SuppressiveFire(PlayerSkillModule context, SkillData data) : base(context, data)
    {
        animator = context.MyAnimModule;
        combatModule = context.MyCombatModule;
    }

    public bool RequiresAmmo => suppressiveData?.RequiresAmmo?? true;

    public bool HasEnoughAmmo()
    {
        return !RequiresAmmo || context.MyWeapon.HasAmmoInMagazine();
    }

    public override bool CanActivate()
    {
        return base.CanActivate() && HasEnoughAmmo();
    }

    public override bool IsValidTarget(RaycastHit hit, out GameObject target, out Vector3 point)
    {
        target = null;

        if (hit.collider == null)
        {
            point = Vector3.zero;
            return false;
        }

        if (((1 << hit.collider.gameObject.layer) & TargetLayer.value) == 0)
        {
            point = Vector3.zero;
            return false;
        }

        point = hit.point;
        return true;
    }

    public override bool CanExecute(SkillContext skillContext)
    {
        return suppressiveData != null && HasEnoughAmmo();
    }

    public override void OnCastingStart(SkillContext skillContext)
    {
        animator.SetAiming(true, null);
    }

    public override void OnExecuteStart(SkillContext skillContext)
    {
        elapsed = 0f;
        tickTimer = suppressiveData.TickInterval;
        context.MyPlayerController.PlayerMove(context.transform.position, false);
        
        float maxAngle = FireRandomBulletAngleInArea(skillContext);
        animator.StartSuppressiveFireAnimation(maxAngle);

    }

    public override void OnExecuteUpdate(SkillContext skillContext, float deltaTime)
    {
        elapsed += deltaTime;
        tickTimer += deltaTime;

        if (tickTimer < suppressiveData.TickInterval)
            return;

        tickTimer = 0f;
        FireTick(skillContext);
    }

    public override bool IsExecutionFinished(SkillContext skillContext)
    {
        return suppressiveData == null ||
            elapsed >= suppressiveData.Duration ||
            (RequiresAmmo && !context.MyWeapon.HasAmmoInMagazine());
    }
    public override void OnExecuteEnd(SkillContext skillContext)
    {
        animator.StopSuppressiveFireAnimation();
    }

    public override void Execute(SkillContext skillContext) { }

    public override float CalCulateFinalDamage()
    {
        return skillData.BaseDamage;
    }

    public override float CalculateFinalRange()
    {
        return context.MyWeapon.Range - AreaWidth;
    }

    private void FireTick(SkillContext skillContext)
    {
        if (RequiresAmmo && !context.MyWeapon.CanFire(true))
            return;

        Vector3 origin = context.transform.position;
        Vector3 targetCenter = skillContext.CastedPosition;

        Vector3 fireDirection = targetCenter - origin;
        fireDirection.y = 0f;

        float targetDistance = fireDirection.magnitude;
        if (targetDistance < 0.001f)
            return;

        fireDirection.Normalize();

        ApplySuppressiveFireDamage(origin, targetCenter, fireDirection, targetDistance, suppressiveData.AoERadius);
    }

    private void ApplySuppressiveFireDamage(Vector3 origin, Vector3 targetCenter, Vector3 fireDirection, float targetDistance, float aoeRadius)
    {
        damagedTargets.Clear();

        float queryLength = targetDistance + aoeRadius;
        Vector3 boxCenter = origin + fireDirection * (queryLength * 0.5f);
        Vector3 halfExtents = new Vector3(aoeRadius, 3f, queryLength * 0.5f);

        Quaternion boxRotation = Quaternion.LookRotation(fireDirection, Vector3.up);

        int count = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, hits, boxRotation, TargetLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider hit = hits[i];
            if (hit == null) continue;

            Vector3 targetPos = hit.bounds.center;

            if (!IsInsideSuppressiveFireArea(origin, targetCenter, fireDirection, targetDistance, aoeRadius, targetPos))
            {
                continue;
            }

            IDamageable damageable = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();

            if (damageable != null && damagedTargets.Add(damageable))
            {
                damageable.TakeDamage(skillData.BaseDamage);
            }
        }
    }

    private float FireRandomBulletAngleInArea(SkillContext skillContext)
    {
        float coneHeight = skillContext.FinalRange;
        float coneEndWidth = suppressiveData.AoERadius;

        return coneHeight > 0.001f
            ? Mathf.Atan2(coneEndWidth, coneHeight) * Mathf.Rad2Deg
            : 0f;
    }

    private bool IsInsideSuppressiveFireArea(Vector3 origin, Vector3 targetCenter, Vector3 fireDirection, float targetDistance, float aoeRadius, Vector3 targetPosition)
    {
        origin.y = 0f;
        targetCenter.y = 0f;
        targetPosition.y = 0f;

        float sqrDistanceToAoeCenter = (targetPosition - targetCenter).sqrMagnitude;
        if (sqrDistanceToAoeCenter <= aoeRadius * aoeRadius)
            return true;

        Vector3 toTarget = targetPosition - origin;

        float forwardDistance = Vector3.Dot(toTarget, fireDirection);
        if (forwardDistance < 0f || forwardDistance > targetDistance)
            return false;

        Vector3 closestPointOnCenterLine = origin + fireDirection * forwardDistance;
        Vector3 lateralOffset = targetPosition - closestPointOnCenterLine;
        float allowedRadius = aoeRadius * (forwardDistance / targetDistance);

        return lateralOffset.sqrMagnitude <= allowedRadius * allowedRadius;
    }
}
