using Unity.Netcode;
using UnityEngine;

public class EnemyProjectile : NetworkBehaviour
{
    [SerializeField]
    private int damage = 5;

    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (IsServer)
        {
            rigid.linearVelocity = velocity;
            ClientSetVelocityRpc(velocity);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ClientSetVelocityRpc(Vector2 velocity)
    {
        if (!IsHost)
        {
            rigid.linearVelocity = velocity;
        }
    }

    private void DestroyProjectile()
    {
        if (!NetworkObject.IsSpawned) return;

        NetworkObject.Despawn(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        GameObject otherObject = collision.gameObject;

        if (otherObject.CompareTag("Wall") || otherObject.CompareTag("Obstacle"))
        {
            DestroyProjectile();
            return;
        }

        if (otherObject.CompareTag("Player") && otherObject.TryGetComponent<LivingEntity>(out LivingEntity livingEntity))
        {
            // livingEntity.TakeDamage(damage);
            DestroyProjectile();
        }
    }
}
