using System.Collections;
using UnityEngine;

public class ProjectileRock : ThrowProjectileBase
{
    [SerializeField] private float destroyDelay = 2f;
    private Coroutine destroyCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }
    
    private void OnDisable()
    {
        if(destroyCoroutine != null) StopCoroutine(destroyCoroutine);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (hasLanded)
        {
            destroyCoroutine = StartCoroutine(DestroyCoroutine());
        }
    }

    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, impactNoiseRadius);
    }
#endif
}
