using UnityEngine;
using System.Collections;

public class ProjectileGrenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private Rigidbody myRigid;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] public int ExplodeDamage 
    {
        get {return ExplodeDamage; }
        set {ExplodeDamage = value < 0 ? 0 : value; }
    }
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] private float fuseTime = 3f;

    private Coroutine explosionCoroutine;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        Throw(Vector3.zero);
    }

    private void OnDisable()
    {
        if(explosionCoroutine != null)
        {
            StopCoroutine(explosionCoroutine);
        }
    }

    public void Throw(Vector3 velocity)
    {
        myRigid.AddForce(velocity, ForceMode.VelocityChange);
        explosionCoroutine = StartCoroutine(FuseCountdown());
    }

    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
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
                damageable.TakeDamage(ExplodeDamage);
            }
        }
        Destroy(gameObject);
    }
}
