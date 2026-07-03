using UnityEngine;
using System.Collections;

public class ProjectileGrenade : ThrowProjectileBase
{
    [Header("Grenade Settings")]
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] public int ExplodeDamage{get; private set;}
    [SerializeField] public float ExplosionRadius{get; private set;}
    [SerializeField] int groundLayer;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionNoiseRadius = 18f;

    //시간초 UI 넣는건?

    private Coroutine explosionCoroutine;
    private bool hasLanded;
    private bool hasExploded;

    protected override void Awake()
    {
        base.Awake();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    protected void OnDisable()
    {
        if(explosionCoroutine != null) StopCoroutine(explosionCoroutine);
    }

    public void SetUp(int damage, float radius, float fuseTime, float explosionNoiseRadius)
    {
        ExplodeDamage = damage;
        ExplosionRadius = radius;
        this.fuseTime = fuseTime;
        this.explosionNoiseRadius = explosionNoiseRadius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(hasLanded || collision.gameObject.layer != groundLayer) return;

        hasLanded = true;
        EmitNoise(impactNoiseRadius);

        if (myRigid != null)
        {
            myRigid.linearVelocity = Vector3.zero;
            myRigid.angularVelocity = Vector3.zero;
        }

        explosionCoroutine = StartCoroutine(FuseCountdown());
    }

    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        EmitNoise(explosionNoiseRadius);

        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
        }
        if (explosionSound != null)
        {
            SoundManager soundManager = Managers.Instance?.SoundManager;
            if(soundManager != null) soundManager.PlayOneShotAt(explosionSound, transform.position);
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius, effectedEntityLayer);
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
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactNoiseRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionNoiseRadius);
    }
#endif
}
