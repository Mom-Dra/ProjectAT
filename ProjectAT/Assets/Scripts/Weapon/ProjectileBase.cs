using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class ThrowProjectileBase : MonoBehaviour, IThrowableProjectile
{
    [Header("Base Projectile Settings")]
    [SerializeField] protected Rigidbody myRigid;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] protected LayerMask effectedEntityLayer = ~0;
    [SerializeField] protected float initialOwnerCollisionIgnoreTime = 0.35f;
    [SerializeField] protected float impactNoiseRadius = 6f;

    protected virtual void Awake()
    {
        if(myRigid is null) myRigid = GetComponent<Rigidbody>();
    }

    public virtual void Throw(Vector3 velocity)
    {
        if (myRigid == null) myRigid = GetComponent<Rigidbody>();
        myRigid.linearVelocity = velocity;
    }

    public virtual void Setup(float impactNoiseRadius, LayerMask effectedEntityLayer)
    {
        this.impactNoiseRadius = impactNoiseRadius;
        this.effectedEntityLayer = effectedEntityLayer;
    }

    public void IgnoreCollisionWith(GameObject owner)
    {
        if(owner == null || initialOwnerCollisionIgnoreTime <= 0f) return;

        Collider[] projectileColliders = GetComponentsInChildren<Collider>();
        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        if(projectileColliders.Length == 0 || ownerColliders.Length == 0) return;

        foreach(Collider col in projectileColliders)
        {
            foreach(Collider ownCol in ownerColliders)
            {
                if(col == null || ownCol == null) continue;
                Physics.IgnoreCollision(col, ownCol, true);
            }
        }
        StartCoroutine(RestoreCollisionAfterDelay(projectileColliders, ownerColliders));
    }

    private IEnumerator RestoreCollisionAfterDelay(Collider[] projectileColliders, Collider[] ownerColliders)
    {
        yield return new WaitForSeconds(initialOwnerCollisionIgnoreTime);

        foreach(Collider col in projectileColliders)
        {
            foreach(Collider ownCol in ownerColliders)
            {
                if(col == null || ownCol == null) continue;
                Physics.IgnoreCollision(col, ownCol, false);
            }
        }
    }

    protected virtual void EmitNoise(float radius)
    {
        if (radius <= 0f) return;

        Collider[] noiseHits = Physics.OverlapSphere(transform.position, radius, effectedEntityLayer, QueryTriggerInteraction.Ignore);
        HashSet<INoiseDetector> notifiedDetectors = new HashSet<INoiseDetector>(); //여러개의 Collider를 가진 Enemy일 경우 중복 감지 방지

        foreach (Collider noiseHit in noiseHits)
        {
            INoiseDetector detector = noiseHit.GetComponentInParent<INoiseDetector>();
            if (detector == null) continue;
            if (!notifiedDetectors.Add(detector)) continue;

            detector.OnNoiseDetect(transform.position);
        }

        SoundManager soundManager = Managers.Instance?.SoundManager;
        if(soundManager != null)
        {
            soundManager.PlayOneShotAt(impactSound, transform.position);
        }
    }
}
