using EPOOutline;
using NUnit.Framework;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

public class PlayerCombatModule : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponHolder myWeapon;
    [SerializeField] private Transform firePoint;
    //[SerializeField] private GameObject bulletPrefab;

    [Header("Params")]
    [SerializeField] private LayerMask enemyLayer;
    private Collider[] enemyColliderBuffer = new Collider[8];
    private float LastFireTime;

    private void Awake()
    {
        InitiateComponents();
        InitiateParams();
    }

    private void InitiateComponents()
    {
        myWeapon = GetComponentInChildren<WeaponHolder>();
    }

    private void InitiateParams()
    {
        //enemyLayer = LayerMask.GetMask("Enemy");
    }

    public bool IsEnemyInRange(Enemy enemy)
    {
        return (enemy.transform.position - transform.position).sqrMagnitude <= myWeapon.Range * myWeapon.Range
            && CheckEnemyVisibility(enemy);
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

    private bool CheckEnemyVisibility(Enemy targetEnemy)
    {
        Vector3 directionToEnemy = (targetEnemy.transform.position - firePoint.position).normalized;
        Ray ray = new Ray(firePoint.position, directionToEnemy);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (hitInfo.collider.GetComponent<Enemy>() == targetEnemy)
            {
                return true;
            }
        }
        return false;
    }

/*    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, myWeapon.Range);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * myWeapon.Range);
    }*/

    public bool CanFire()
    {
        return Time.time - LastFireTime > myWeapon.FireRate;
    }


    public void NormalAttackEnemy(Enemy target)
    {
        Debug.Log($"Player Attack : {target.gameObject.name}");
        LastFireTime = Time.time;
        //Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        //target.TakeDamage(myWeapon.Damage);
    }

    /*public void ChangeWeapon(GunData newWeapon) //나중에 총기스왑 구현되면 그때 ㄱ
    {
        myWeapon.SetWeapon(newWeapon);
    }*/
}
