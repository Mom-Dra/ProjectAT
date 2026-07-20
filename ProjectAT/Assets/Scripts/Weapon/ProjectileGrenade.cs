using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileGrenade : ThrowProjectileBase
{
    [Header("Grenade Settings")]
    [SerializeField] private ParticleSystem explosionEffect;
    public int ExplodeDamage{get; private set;}
    public float ExplosionRadius{get; private set;}
    private float fuseTime = 3f;
    private float explosionNoiseRadius = 18f;
    private List<BuffData> buffsToApplyOnExplosion;

    private Coroutine explosionCoroutine;
    private float remainingFuseTime = -1f;
    private bool hasExploded;

    public bool IsFuseRunning => remainingFuseTime >= 0f && !hasExploded;
    public float RemainingFuseTime => IsFuseRunning ? remainingFuseTime : fuseTime;

    protected override void Awake()
    {
        base.Awake();
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

    public void SetUpBuffData(BuffData[] buffs)
    {
        buffsToApplyOnExplosion = new List<BuffData>(buffs);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (hasLanded)
        {
            remainingFuseTime = fuseTime;
            explosionCoroutine = StartCoroutine(FuseCountdown());
        }
    }

    private IEnumerator FuseCountdown()
    {
        while (remainingFuseTime > 0f)
        {
            remainingFuseTime -= Time.deltaTime;
            yield return null;
        }

        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Physics.SyncTransforms();

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

            BuffModule buffModule = hitCollider.GetComponent<BuffModule>();
            if (buffModule != null)
            {
                ApplyBuffsOnExplosion(buffModule);
            }
        }
        
        if (explosionEffect != null)
        {
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
        }

        EmitNoise(explosionNoiseRadius);
        soundController.PlayExplosionAt(transform.position);

        Destroy(gameObject);
    }

    protected void ApplyBuffsOnExplosion(BuffModule targetModule)
    {
        if (buffsToApplyOnExplosion == null || buffsToApplyOnExplosion.Count == 0)
        {
            return;
        }

        foreach (BuffData buffData in buffsToApplyOnExplosion)
        {
            targetModule.AddBuff(buffData);
        }
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
