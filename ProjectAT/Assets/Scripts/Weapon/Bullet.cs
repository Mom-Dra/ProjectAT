using System.Collections;
using System.Diagnostics.Tracing;
using System.Security.Cryptography.X509Certificates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    private Vector3 destination;
    private Rigidbody rb;

    private PooledObject pooledObject;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(Vector3 destination, float lifetime)
    {
        this.destination = destination;

        StartCoroutine(BulletDestroyCoroutine(lifetime));
    }

    private void FixedUpdate()
    {
        if (Vector3.SqrMagnitude(rb.position - destination) < 0.7f)
            DestroyBullet();
    }

    private IEnumerator BulletDestroyCoroutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        DestroyBullet();
    }

    private void DestroyBullet()
    {
        rb.linearVelocity = Vector3.zero;
        gameObject.SetActive(false);

        pooledObject.ReturnToPool();
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }
}
