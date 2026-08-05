using UnityEngine;

public class PlayerCombatModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHolder myWeapon;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private Transform throwPoint;

    [Header("Params")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float arcHeight = 2.0f;
    [SerializeField] private float AimingCoolTime = 0.5f;
    [SerializeField] private float currentAimingTime = 0f;

    #region Properties
    public bool IsAiming { get; private set; }
    public float ThrowRange => myStatus.ThrowRange;
    public WeaponHolder MyWeapon => myWeapon;
    public Transform ThrowPoint => throwPoint;
    public Transform EyePoint => eyePoint;
    #endregion

    private void Awake()
    {
        InitiateComponents();
    }

    private void InitiateComponents()
    {
        myWeapon = GetComponentInChildren<WeaponHolder>();
        myStatus = GetComponent<EntityStatus>();
    }
    
    private void Update()
    {
        if (IsAiming)
        {
            currentAimingTime = Mathf.Min(currentAimingTime + Time.deltaTime, AimingCoolTime);
        }
    }

    public bool IsEnemyInWeaponRange(Enemy enemy, float rangeOffset = 0f)
    {
        return enemy != null && myWeapon != null && CheckPositionInRange(enemy.transform.position, myWeapon.Range + rangeOffset);
    }

    public bool IsTargetVisible(Collider targetCollider)
    {
        return targetCollider != null && CheckTargetVisibility(targetCollider, eyePoint);
    }

    private bool CheckPositionInRange(Vector3 pos, float range)
    {
        return (pos - transform.position).sqrMagnitude <= range * range;
    }

    private bool CheckTargetVisibility(Collider targetCollider, Transform baseTf)
    {
        if (targetCollider == null || baseTf == null)
        {
            Debug.LogWarning($"{nameof(CheckTargetVisibility)}: targetCollider or baseTf is null.");
            return false;
        }

        return IsTargetVisibleFrom(targetCollider, targetCollider.bounds.center, enemyLayer, baseTf.position);
    }

    public bool IsTargetInWeaponSight(Collider targetCollider, LayerMask targetLayer)
    {
        if (targetCollider == null || myWeapon == null) return false;

        return CheckPositionInRange(targetCollider.transform.position, myWeapon.Range)
            && IsTargetVisible(targetCollider, targetLayer);
    }

    public bool IsTargetVisible(Collider targetCollider, LayerMask targetLayer)
    {
        if (targetCollider == null) return false;

        return IsTargetVisibleFrom(targetCollider, targetCollider.bounds.center, targetLayer, eyePoint.position);
    }

    public bool IsTargetVisible(Collider targetCollider, Vector3 targetPoint, LayerMask targetLayer)
    {
        if (targetCollider == null) return false;

        return IsTargetVisibleFrom(targetCollider, targetPoint, targetLayer, eyePoint.position);
    }

    private bool IsTargetVisibleFrom(Collider targetCollider, Vector3 targetPoint, LayerMask targetLayer, Vector3 origin)
    {
        Vector3 directionToTarget = targetPoint - origin;
        float distance = directionToTarget.magnitude;

        if (distance <= Mathf.Epsilon) return true;
        if (distance > myStatus.MaxViewingDistance) return false;

        int visibilityMask = targetLayer.value | obstacleLayer.value;

        if (!Physics.Raycast(origin, directionToTarget / distance, out RaycastHit hitInfo, distance, visibilityMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return IsHitTarget(hitInfo, targetCollider.gameObject);
    }

    private bool IsHitTarget(RaycastHit hitInfo, GameObject target)
    {
        if (hitInfo.collider == null || target == null) return false;

        Transform hitTransform = hitInfo.collider.transform;
        Transform targetTransform = target.transform;

        if (hitTransform == targetTransform || hitTransform.IsChildOf(targetTransform)) return true;
        return false;
    }

    public bool CheckAimingTargetEnough()
    {
        return currentAimingTime >= AimingCoolTime;
    }

    public bool CheckWeaponFireReady()
    {
        return myWeapon.CanFire();
    }

    public bool HasNormalAttackAmmo()
    {
        return myWeapon != null && myWeapon.HasAnyAmmo();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, myWeapon.Range);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + eyePoint.forward * myWeapon.Range);
    }

    public void NormalAttackEnemy(Enemy target)
    {
        NormalAttackTarget(target.gameObject);
    }

    public void NormalAttackTarget(GameObject target)
    {
        if (target.TryGetComponent(out IDamageable damageable))
        {
            myWeapon.FireWeaponOnlyVFX(target.transform.position, Vector3.up * 1.5f, false);
            damageable.TakeDamage(myWeapon.Damage);
        }
    }

    public void SetAiming(bool isAiming)
    {
        if (IsAiming == isAiming) return;

        IsAiming = isAiming;
        currentAimingTime = 0f;
    }

    public bool CanThrowSomethingToPosition(Collider projectileObjectCollider, Vector3 position)
    {
        if (projectileObjectCollider == null) return false;
        if (Vector3.SqrMagnitude(position - throwPoint.position) > myStatus.ThrowRange * myStatus.ThrowRange)
        {
            return false;
        }

        Vector3 origin = throwPoint.position;

        if (!PhysicsMathUtility.CalculateTrajectory(origin, position, arcHeight, out Vector3 initialVelocity, out float totalTime))
        {
            return false;
        }

        float radius = 0.1f;
        if (projectileObjectCollider != null)
        {
            radius = Mathf.Max(projectileObjectCollider.bounds.extents.x, projectileObjectCollider.bounds.extents.z);
        }

        int segmentCount = 20;
        float deltaTime = totalTime / segmentCount;
        Vector3 previousPoint = origin;

        for (int i = 1; i <= segmentCount; i++)
        {
            float t = i * deltaTime;
            Vector3 nextPoint = origin + (initialVelocity * t) + (0.5f * Physics.gravity * t * t);
            Vector3 direction = nextPoint - previousPoint;

            if (Physics.SphereCast(previousPoint, radius, direction.normalized, out RaycastHit _, direction.magnitude, obstacleLayer))
            {
                return false;
            }

            previousPoint = nextPoint;
        }

        return true;
    }

    public void ThrowSomthingToTarget(ThrowProjectileBase throwingObject, Vector3 targetPos)
    {
        if (throwingObject == null) return;

        throwingObject.gameObject.transform.position = throwPoint.position;
        Vector3 origin = throwPoint.position;

        if (PhysicsMathUtility.CalculateTrajectory(origin, targetPos, arcHeight, out Vector3 velocity, out float time))
        {
            throwingObject.IgnoreCollisionWith(gameObject);
            throwingObject.Throw(velocity);
        }
    }

    public void RequestWeaponReload()
    {
        myWeapon.ReloadingWeapon();
    }

    public void RequestCancelReload()
    {
        myWeapon.CancelReloadingWeapon();
    }
}
