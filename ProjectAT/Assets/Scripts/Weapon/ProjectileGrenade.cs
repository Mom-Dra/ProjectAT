using UnityEngine;
using System.Collections;

public class ProjectileGrenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private Rigidbody myRigid;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explodeDamage = 50;
    [SerializeField] public int ExplodeDamage 
    {
        get { return explodeDamage; }
        set { explodeDamage = value < 0 ? 0 : value; }
    }
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float arcHeight = 2.0f;     // 수류탄이 날아갈 때의 최고 높이

    private Coroutine explosionCoroutine;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
    }


    // private void OnDisable()
    // {
    //     if(explosionCoroutine != null)
    //     {
    //         StopCoroutine(explosionCoroutine);
    //     }
    // }

    public void Throw(Vector3 targetPos)
    {
        if (myRigid == null) myRigid = GetComponent<Rigidbody>();

        // 1. 물리 계산을 통해 필요한 속도 벡터 구하기
        Vector3 velocity = CalculateVelocity(transform.position, targetPos, arcHeight);

        // 2. 리지드바디에 속도 적용 (질량 무시하고 즉시 속도 변경)
        myRigid.AddForce(velocity, ForceMode.VelocityChange);

        explosionCoroutine = StartCoroutine(FuseCountdown());
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
             height = displacementY + 1f; // 목표보다 1단위 더 높게 설정
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

    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        Debug.Log("Grenade Exploded");
        Explode();
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);
        foreach (Collider hitCollider in hitColliders)
        {
            Physics.Raycast(transform.position, (hitCollider.transform.position - transform.position).normalized, out RaycastHit hitInfo, explosionRadius);
            if(hitInfo.collider != hitCollider)
            {
                continue;
            }
            
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explodeDamage);
            }
        }
        Debug.Log("Grenade Explosion Processed");
        Destroy(gameObject);
    }
}
