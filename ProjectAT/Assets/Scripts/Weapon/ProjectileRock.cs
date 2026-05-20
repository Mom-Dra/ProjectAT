using System.Collections;
using UnityEngine;

public class ProjectileRock : ProjectileBase
{
    [SerializeField] private int groundLayer;
    [SerializeField] private float destroyDelay = 2f;
    private Coroutine destroyCoroutine;
    private bool hasLanded;

    protected override void Awake()
    {
        base.Awake();
        groundLayer = LayerMask.NameToLayer("Ground");
    }
    private void OnDisable()
    {
        if(destroyCoroutine != null) StopCoroutine(destroyCoroutine);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;
        if (collision.gameObject.layer != groundLayer) return;

        hasLanded = true;
        EmitNoise(impactNoiseRadius);

        if (myRigid != null)
        {
            myRigid.linearVelocity = Vector3.zero;
            myRigid.angularVelocity = Vector3.zero;
        }

        destroyCoroutine = StartCoroutine(DestroyCoroutine());
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
