using UnityEngine;
using System.Collections;

public class ProjectileGrenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private Rigidbody myRigid;
    [SerializeField] public int ExplodeDamage{get; private set;}
    [SerializeField] public float ExplosionRadius{get; private set;}
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] int groundLayer;
    [SerializeField] private float fuseTime = 3f;

    private Coroutine explosionCoroutine;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }


    // private void OnDisable()
    // {
    //     if(explosionCoroutine != null)
    //     {
    //         StopCoroutine(explosionCoroutine);
    //     }
    // }

    public void SetUp(int damage, float radius, float fuse, LayerMask damageableLayers)
    {
        ExplodeDamage = damage;
        ExplosionRadius = radius;
        fuseTime = fuse;
        this.damageableLayers = damageableLayers;
    }

    public void Throw(Vector3 velocity)
    {
        if (myRigid == null) myRigid = GetComponent<Rigidbody>();

        myRigid.AddForce(velocity, ForceMode.VelocityChange);
        explosionCoroutine = StartCoroutine(FuseCountdown());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == groundLayer)
        {
            myRigid.linearVelocity = Vector3.zero;
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

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius, damageableLayers);
        foreach (Collider hitCollider in hitColliders)
        {
            Physics.Raycast(transform.position, (hitCollider.transform.position - transform.position).normalized, out RaycastHit hitInfo, ExplosionRadius);
            if(hitInfo.collider != hitCollider)
            {
                continue;
            }
            
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(ExplodeDamage);
            }
        }
        Debug.Log("Grenade Explosion Processed");
        Destroy(gameObject);
    }
}
