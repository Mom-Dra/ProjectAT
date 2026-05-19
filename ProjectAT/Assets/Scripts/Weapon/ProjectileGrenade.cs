using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileGrenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionEffect;
    [SerializeField] private Rigidbody myRigid;
    [SerializeField] public int ExplodeDamage{get; private set;}
    [SerializeField] public float ExplosionRadius{get; private set;}
    [SerializeField] private LayerMask damageableLayers;
    [SerializeField] int groundLayer;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float impactNoiseRadius = 6f;
    [SerializeField] private float explosionNoiseRadius = 18f;
    [SerializeField] private LayerMask noiseDetectorLayers = ~0;
    [SerializeField] private float initialOwnerCollisionIgnoreTime = 0.35f;
    //시간초 UI 넣는건?

    private Coroutine explosionCoroutine;
    private bool hasLanded;
    private bool hasExploded;

    private void Awake()
    {
        myRigid = GetComponent<Rigidbody>();
        groundLayer = LayerMask.NameToLayer("Ground");
    }

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
        myRigid.linearVelocity = velocity;
    }

    public void IgnoreCollisionWith(GameObject owner)
    {
        if (owner == null || initialOwnerCollisionIgnoreTime <= 0f) return;

        Collider[] grenadeColliders = GetComponentsInChildren<Collider>();
        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        if (grenadeColliders.Length == 0 || ownerColliders.Length == 0) return;

        foreach (Collider grenadeCollider in grenadeColliders)
        {
            foreach (Collider ownerCollider in ownerColliders)
            {
                if (grenadeCollider == null || ownerCollider == null) continue;

                Physics.IgnoreCollision(grenadeCollider, ownerCollider, true);
            }
        }

        StartCoroutine(RestoreCollisionAfterDelay(grenadeColliders, ownerColliders));
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

    private IEnumerator RestoreCollisionAfterDelay(Collider[] grenadeColliders, Collider[] ownerColliders)
    {
        yield return new WaitForSeconds(initialOwnerCollisionIgnoreTime);

        foreach (Collider grenadeCollider in grenadeColliders)
        {
            foreach (Collider ownerCollider in ownerColliders)
            {
                if (grenadeCollider == null || ownerCollider == null) continue;

                Physics.IgnoreCollision(grenadeCollider, ownerCollider, false);
            }
        }
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
        Destroy(gameObject);
    }

    private void EmitNoise(float radius)
    {
        if (radius <= 0f) return;

        Collider[] noiseHits = Physics.OverlapSphere(transform.position, radius, noiseDetectorLayers, QueryTriggerInteraction.Ignore);
        HashSet<INoiseDetector> notifiedDetectors = new HashSet<INoiseDetector>(); //여러개의 Collider를 가진 Enemy일 경우 중복 감지 방지

        foreach (Collider noiseHit in noiseHits)
        {
            INoiseDetector detector = noiseHit.GetComponentInParent<INoiseDetector>();
            if (detector is null) continue;
            if (!notifiedDetectors.Add(detector)) continue;

            detector.OnNoiseDetect(transform.position);
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
