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
    [SerializeField] private float arcHeight = 2.0f;
    [SerializeField] private LayerMask ObstacleLayer;
    [SerializeField] private float AimingCoolTime = 0.5f;
    [SerializeField] private float currentAimingTime = 0f;
    
    #region Properties
    public bool IsAiming {get; private set;}
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


    private void Start()
    {
        Managers.Instance.UIManager.InitPlayerGunInfo(myWeapon);
    }

    private void Update()
    {
        if (IsAiming)
        {
            currentAimingTime = Mathf.Min(currentAimingTime + Time.deltaTime, AimingCoolTime);
        }
    }

    
    public bool IsEnemyInWeaponSight(Enemy enemy, float rangeOffset = 0f)
    {
        return CheckPositionInRange(enemy.transform.position, myWeapon.Range + rangeOffset)
            && CheckEnemyVisibility(enemy, eyePoint);
    }

    private bool CheckPositionInRange(Vector3 pos, float range)
    {
        return (pos - transform.position).sqrMagnitude <= range * range;
    }

    private bool CheckEnemyVisibility(Enemy targetEnemy, Transform baseTf)
    {
        Vector3 directionToEnemy = targetEnemy.transform.position - baseTf.position;
        directionToEnemy.y = 0.0f;

        Ray ray = new Ray(baseTf.position, directionToEnemy);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, myStatus.MaxViewingDistance, enemyLayer))
        {
            if (hitInfo.collider.gameObject == targetEnemy.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckTargetVisibility(GameObject target)
    {
        return CheckPositionVisibility(target.transform.position);
    }

    public bool CheckPositionVisibility(Vector3 position)
    {
        Vector3 directionToTarget = position - eyePoint.position;
        directionToTarget.y = eyePoint.position.y;

        if (Physics.Raycast(eyePoint.position, directionToTarget, directionToTarget.magnitude, ObstacleLayer))
        {
            return false;
        }
        return true;
    }

    public bool IsTargetInWeaponSight(GameObject target)
    {
        return CheckPositionInRange(target.transform.position, myWeapon.Range) && CheckTargetVisibility(target);
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

    public bool CanThrowSomethingToPosition(GameObject projectileObject, Vector3 position)
    {
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
        if (projectileObject != null)
        {
            Collider col = projectileObject.GetComponentInChildren<Collider>();
            if (col != null) radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
        }

        int segmentCount = 20; 
        float deltaTime = totalTime / segmentCount;
        Vector3 previousPoint = origin;

        for (int i = 1; i <= segmentCount; i++)
        {
            float t = i * deltaTime;
            Vector3 nextPoint = origin + (initialVelocity * t) + (0.5f * Physics.gravity * t * t);
            Vector3 direction = nextPoint - previousPoint;

            if (Physics.SphereCast(previousPoint, radius, direction.normalized, out RaycastHit hit, direction.magnitude, ObstacleLayer))
            {
                Debug.Log($"Chase State : Trajectory blocked by {hit.collider.gameObject.name} at {hit.point}");
                return false;
            }

            previousPoint = nextPoint;
        }

        return true;
    }

    public void ThrowSomthingToTarget(ThrowProjectileBase throwingObject, Vector3 targetPos)
    {
        if(throwingObject == null) return;

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
