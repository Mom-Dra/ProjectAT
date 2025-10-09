using System.Collections;
using System.Security.Cryptography.X509Certificates;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    private Vector3 destination;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 destination, float lifetime)
    {
        this.destination = destination;

        StartCoroutine(BulletDestroyCoroutine(lifetime));
    }

    //public override void OnNetworkDespawn()
    //{
    //    //if (IsClient)
    //    //{
    //    //    ParticleSystem explosionParticles = ExplosionsPool.s_Singleton.Pool.Get();
    //    //    explosionParticles.transform.position = transform.position;
    //    //    explosionParticles.Play();
    //    //}
    //}

    //private void Update()
    //{
    //    if (IsServer && Vector3.SqrMagnitude(transform.position - destination) < 0.5f)
    //        DestroyBullet();
    //}

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
    }

    public void SetVelocity(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    //[Rpc(SendTo.ClientsAndHost)]
    //private void ClientSetVelocityRpc(Vector3 velocity)
    //{
    //    rb.linearVelocity = velocity;
    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (!NetworkManager.Singleton.IsServer || !NetworkObject.IsSpawned)
    //    {
    //        return;
    //    }

    //    GameObject collisionObject = collision.gameObject;

    //    if(collisionObject.TryGetComponent(out IDamageable damageable))
    //    {
    //        damageable.TakeDamage(damage);
    //    }
    //}

    //private void OnCollisionEnter(Collision2D other)
    //{
    //    var otherObject = other.gameObject;

    //    if (!NetworkManager.Singleton.IsServer || !NetworkObject.IsSpawned)
    //    {
    //        return;
    //    }

    //    if()

    //    //if (otherObject.TryGetComponent<Asteroid>(out var asteroid))
    //    //{
    //    //    asteroid.Explode();
    //    //    DestroyBullet();
    //    //    return;
    //    //}

    //    //if (m_Bounce == false && (otherObject.CompareTag("Wall") || otherObject.CompareTag("Obstacle")))
    //    //{
    //    //    DestroyBullet();
    //    //}

    //    //if (otherObject.TryGetComponent<ShipControl>(out var shipControl))
    //    //{
    //    //    if (shipControl != m_Owner)
    //    //    {
    //    //        shipControl.TakeDamage(m_Damage);
    //    //        DestroyBullet();
    //    //    }
    //    //}
    //}
}
