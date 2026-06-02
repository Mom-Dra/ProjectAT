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
    private IPerceivable attacker;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    public void SetUp(int damage, float radius, float fuse, LayerMask damageableLayers, IPerceivable attacker = null)
    {
        ExplodeDamage = damage;
        ExplosionRadius = radius;
        fuseTime = fuse;
        this.damageableLayers = damageableLayers;
        this.attacker = attacker;
    }

    public void Throw(Vector3 velocity)
    {
        if (myRigid == null) myRigid = GetComponent<Rigidbody>();
        myRigid.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == groundLayer)
        {
            myRigid.linearVelocity = Vector3.zero;
            myRigid.angularVelocity = Vector3.zero;
            explosionCoroutine = StartCoroutine(FuseCountdown());
        }
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

                if (attacker is not null && hitCollider.TryGetComponent(out Enemy enemy))
                    enemy.ReceiveAttack(attacker);
            }
        }
        Destroy(gameObject);
    }
}
