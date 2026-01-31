using Unity.Behavior;
using UnityEngine;

public class PlayerCombatModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHolder myWeapon;
    [SerializeField] private Transform firePoint;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private Transform throwPoint;
    //[SerializeField] private GameObject bulletPrefab;

    [Header("Params")]
    [SerializeField] private LayerMask enemyLayer;
    private Collider[] enemyColliderBuffer = new Collider[8];
    private float LastFireTime;
    [SerializeField] private float arcHeight = 2.0f; //투사체의 최대 높이
    [SerializeField] private LayerMask ObstacleLayer;

    private void Awake()
    {
        InitiateComponents();
        InitiateParams();
    }

    private void InitiateComponents()
    {
        myWeapon = GetComponentInChildren<WeaponHolder>();
        myStatus = GetComponent<EntityStatus>();
    }

    private void InitiateParams()
    {
        //enemyLayer = LayerMask.GetMask("Enemy");
        //throwPoint = transform.GetChild(2);
    }

    public bool IsEnemyInWeaponSight(Enemy enemy)
    {
        return CheckPositionInRange(enemy.transform.position, myWeapon.Range)
            && CheckEnemyVisibility(enemy);
    }

    private bool CheckPositionInRange(Vector3 pos, float range)
    {
        return (pos - transform.position).sqrMagnitude <= range * range;
    }

    private bool CheckEnemyVisibility(Enemy targetEnemy)
    {
        Vector3 directionToEnemy = targetEnemy.transform.position - firePoint.position;
        directionToEnemy.y = 0.0f;

        Ray ray = new Ray(firePoint.position, directionToEnemy);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, myStatus.MaxViewingDistance, ObstacleLayer))
        {
            if (hitInfo.collider.gameObject == targetEnemy.gameObject)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckPositionVisibility(Vector3 targetPos) //중복되는 부분이 있다. 리펙토링 고려
    {
        Ray ray = new Ray(throwPoint.position, targetPos - throwPoint.position);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, myStatus.MaxViewingDistance, ObstacleLayer))
        {
            if ((hitInfo.point - targetPos).sqrMagnitude < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    public bool CanFire()
    {
        return Time.time - LastFireTime > myWeapon.FireRate 
        && myWeapon.IsAmmoLoaded();
    }

    public Enemy FindClosestEnemy()
    {
        Enemy scanned = null;

        if (Physics.OverlapSphereNonAlloc(transform.position, myWeapon.Range, enemyColliderBuffer, enemyLayer.value) > 0)
        {
            for (int i = 0; i < enemyColliderBuffer.Length; ++i)
            {
                if (enemyColliderBuffer[i] != null)
                {
                    scanned = enemyColliderBuffer[i].GetComponent<Enemy>();
                    if(scanned && CheckEnemyVisibility(scanned)) break;
                }
            }
        }

        return scanned;
    }

    
   private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, myWeapon.Range);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * myWeapon.Range);
    }


    public void NormalAttackEnemy(Enemy target)
    {
        Debug.Log($"Player Attack : {target.gameObject.name}");
        LastFireTime = Time.time;
        //Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        if (target.TryGetComponent(out IDamageable damageable))
            damageable.TakeDamage(myWeapon.Damage);
    }
    public bool CanThrowSomethingToPosition(Vector3 position)
    {
        return CheckPositionInRange(position, myStatus.ThrowRange) 
        && CheckPositionVisibility(position);
    }

    public void ThrowSomthingToTarget(GameObject thowingObject, Vector3 targetPos)
    {
        Vector3 velocity = CalculateVelocity(throwPoint.position + Vector3.up, targetPos, arcHeight);        
        GameObject thrownObj = Instantiate(thowingObject, throwPoint.position + Vector3.up, Quaternion.identity);
        thrownObj.GetComponent<ProjectileGrenade>().Throw(velocity);
    }

    /// <summary>
    /// 시작점에서 목표점까지 지정된 높이의 포물선을 그리며 날아가는 속도를 계산합니다.
    /// </summary>
    /// <param name="origin">던지는 위치</param>
    /// <param name="target">목표 위치</param>
    /// <param name="height">포물선의 최고 높이(상대값)</param>
    /// <returns>초기 속도 벡터</returns>
    private Vector3 CalculateVelocity(Vector3 origin, Vector3 target, float height)
    {
        float gravity = Physics.gravity.y; // 중력 (보통 -9.81)
        float displacementY = target.y - origin.y; // 높이 차이
        
        // 수평 평면(XZ)에서의 거리 벡터와 거리값
        Vector3 displacementXZ = new Vector3(target.x - origin.x, 0, target.z - origin.z);
        float time = 0;

        // 높이값 안전장치 (목표점이 내 위치보다 높을 경우, 최소한 그보다는 더 높게 던져야 함)
        if (displacementY >= height)
        {
             height = displacementY; // 목표보다 1단위 더 높게 설정
        }

      
        float timeUp = Mathf.Sqrt(-2 * height / gravity);

        // 내려가는 시간 (최고점에서 목표점까지)
        // sqrt(2 * (dy - h) / g)
        float timeDown = Mathf.Sqrt(2 * (displacementY - height) / gravity);

        time = timeUp + timeDown;

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        
        // 수평 속도(Vxz): 거리 / 시간
        Vector3 velocityXZ = displacementXZ / time;

        return velocityXZ + velocityY;
    }
}
