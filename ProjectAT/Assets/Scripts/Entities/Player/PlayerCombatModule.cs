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
        throwPoint = transform.GetChild(2);
    }

    public bool IsEnemyInWeaponSight(Enemy enemy)
    {
        return CheckEnemyInRange(enemy, myStatus.ThrowRange)
            && CheckEnemyVisibility(enemy);
    }

    private bool CheckEnemyInRange(Enemy enemy, float range)
    {
        return (enemy.transform.position - transform.position).sqrMagnitude <= range * range;
    }

    private bool CheckEnemyVisibility(Enemy targetEnemy)
    {
        Vector3 directionToEnemy = (targetEnemy.transform.position - firePoint.position);
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

    
/*    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, myWeapon.Range);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * myWeapon.Range);
    }
*/

    public void NormalAttackEnemy(Enemy target)
    {
        Debug.Log($"Player Attack : {target.gameObject.name}");
        LastFireTime = Time.time;
        //Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        //target.TakeDamage(myWeapon.Damage);
    }
    public bool CanThrowSomethingToEnemy(Enemy enemy)
    {
        return CheckEnemyInRange(enemy, myStatus.ThrowRange) 
        && CheckEnemyVisibility(enemy);
    }

    public void ThrowSomthingToTarget(GameObject thowingObject, Vector3 targetPos)
    {
        GameObject thrownObj = Instantiate(thowingObject, throwPoint.position + Vector3.up, Quaternion.identity);
        thrownObj.GetComponent<ProjectileGrenade>().Throw(targetPos);
    }
}
