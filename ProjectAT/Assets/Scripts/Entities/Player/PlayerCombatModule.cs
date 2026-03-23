using Unity.Behavior;
using UnityEngine;

public class PlayerCombatModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHolder myWeapon;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private EntityStatus myStatus;
    [SerializeField] private Transform throwPoint;
    //[SerializeField] private GameObject bulletPrefab;

    [Header("Params")]
    [SerializeField] private LayerMask enemyLayer;
    private Collider[] enemyColliderBuffer = new Collider[8];
    private float LastFireTime;
    [SerializeField] private float arcHeight = 2.0f; //투사체의 최대 높이
    [SerializeField] private LayerMask ObstacleLayer;
    [SerializeField] private float AimingCoolTime = 0.5f;
    [SerializeField] private float currentAimingTime = 0f;
    public bool IsAiming {get; private set;}

    public WeaponHolder MyWeapon => myWeapon;
    private Transform WeaponFirePoint => myWeapon.GunHolderTf;

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
    
    private void Start()
    {
        Managers.Instance.UIManager.InitPlayerGunInfo(myWeapon);
    }

    private void Update()
    {
        if(IsAiming)
        {
            currentAimingTime = Mathf.Min(Time.deltaTime + currentAimingTime + 0.1f, AimingCoolTime);
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
        && currentAimingTime >= AimingCoolTime
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
                    if(scanned && CheckEnemyVisibility(scanned, eyePoint)) break;
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
        Gizmos.DrawLine(eyePoint.position, eyePoint.position + eyePoint.forward * myWeapon.Range);
    }


    public void NormalAttackEnemy(Enemy target)
    {
        Debug.Log($"Player Attack : {target.gameObject.name}");

        if (target.TryGetComponent(out IDamageable damageable)) 
        {
            LastFireTime = Time.time;
            myWeapon.FireWeapon();
            damageable.TakeDamage(myWeapon.Damage);
        }
    }

    public void SetAiming(bool IsAiming)
    {
        if(this.IsAiming != IsAiming){
            this.IsAiming = IsAiming;
            if(!IsAiming) currentAimingTime = 0f;
        }
    }

    public bool CanThrowSomethingToPosition(Vector3 position)
    {
        return CheckPositionInRange(position, myStatus.ThrowRange);
        //&& CheckPositionVisibility(position);
    }

    public void ThrowSomthingToTarget(GameObject throwingObject, Vector3 targetPos)
    {
        throwingObject.transform.position = throwPoint.position;
        Vector3 velocity = CalculateVelocity(throwPoint.position + Vector3.up, targetPos, arcHeight);        
        throwingObject.GetComponent<ProjectileGrenade>().Throw(velocity); //projectle이라는 인터페이스같은걸로 바꾸기
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
