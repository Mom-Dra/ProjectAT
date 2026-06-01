using Unity.Behavior;
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
    public bool IsAiming {get; private set;}

    public WeaponHolder MyWeapon => myWeapon;
    public Vector3 ThrowPoint => throwPoint.position;

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
        if(IsAiming)
        {
            currentAimingTime = Mathf.Min(Time.deltaTime + currentAimingTime + 0.1f, AimingCoolTime); //시간을 항상 계산하지말고, IsAiming이 true가 될떄 그 순간을 기록하고, fire를 호출할때 Time.time - currentAiming 을 체크하는 방식 고려하기.
        }
    }

    public bool IsEnemyInWeaponSight(Enemy enemy)
    {
        return CheckPositionInRange(enemy.transform.position, myWeapon.Range)
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

    private bool CheckTargetVisibility(GameObject target, Transform baseTf)
    {
        Vector3 directionToTarget = target.transform.position - baseTf.position;
        directionToTarget.y = baseTf.position.y;

        Ray ray = new Ray(baseTf.position, directionToTarget);
        if (Physics.Raycast(ray, myStatus.MaxViewingDistance, ObstacleLayer))
        {
            return false;
        }
        return true;
    }

    public bool IsTargetInWeaponSight(GameObject target)
    {
        bool condition = CheckPositionInRange(target.transform.position, myWeapon.Range);
        bool condition2 = CheckTargetVisibility(target, eyePoint);

        return condition && condition2;
    }

    public bool CheckAimingTargetEnough()
    {
        return Time.time - currentAimingTime >= AimingCoolTime;
    }

    public bool CheckWeaponFireReady()
    {
        return myWeapon.CanFire();
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
        if (target.TryGetComponent(out IDamageable damageable)) 
        {
            myWeapon.FireWeapon();
            //1damageable.TakeDamage(myWeapon.Damage); //NOTE : FireWeapon에서 이미 데미지를 주는중임. 이 코드 삭제 생각해보기
        }
    }

    public void SetAiming(bool IsAiming)
    {
        if(this.IsAiming != IsAiming){
            this.IsAiming = IsAiming;
            currentAimingTime = IsAiming ? Time.time : 0f;
        }
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

    public void ThrowSomthingToTarget(GameObject throwingObject, Vector3 targetPos)
    {
        throwingObject.transform.position = throwPoint.position;
        Vector3 origin = throwPoint.position;

        if (PhysicsMathUtility.CalculateTrajectory(origin, targetPos, arcHeight, out Vector3 velocity, out float time))
        {
            ProjectileBase grenade = throwingObject.GetComponent<ProjectileBase>();
            if (grenade == null) {
                Debug.LogWarning($"{gameObject.name} : The object to throw does not have a ProjectileBase component.");
                return; 
            }

            grenade.IgnoreCollisionWith(gameObject);
            grenade.Throw(velocity);
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
