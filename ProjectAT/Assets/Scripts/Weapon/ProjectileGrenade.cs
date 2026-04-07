using UnityEngine;
using System.Collections;

public class ProjectileGrenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private Rigidbody myRigid;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explodeDamage = 50;
    [SerializeField]
    public int ExplodeDamage
    {
        get { return explodeDamage; }
        set { explodeDamage = value < 0 ? 0 : value; }
    }
    public float ExplosionRadius { get { return explosionRadius; } set { explosionRadius = value < 0 ? 0.1f : value; } }
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

    public void Throw(Vector3 velocity)
    {
        if (myRigid == null) myRigid = GetComponent<Rigidbody>();

        // 2. 리지드바디에 속도 적용 (질량 무시하고 즉시 속도 변경)
        myRigid.AddForce(velocity, ForceMode.VelocityChange);
        explosionCoroutine = StartCoroutine(FuseCountdown());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            myRigid.isKinematic = true;
        }
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
            if (hitInfo.collider != hitCollider)
            {
                continue;
            }

            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explodeDamage, null);
            }
        }
        Debug.Log("Grenade Explosion Processed");
        Destroy(gameObject);
    }
}
