using Unity.Netcode;
using UnityEngine;

// 사용하지 않음
public class EntityController : NetworkBehaviour
{
    private EntityStatus myStatus;

    private void Awake()
    {
        myStatus = GetComponent<EntityStatus>();
    }

    [Rpc(SendTo.Server)]
    public void TakingHitServerRpc(int damage)
    {
        if (IsServer)
        {
            // myStatus.TakeDamage(damage, myStatus);
            TakingHitClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void TakingHitClientRpc()
    {
        Debug.Log($"{transform.name} : TakingHit!");
    }

    public bool IsAlive()
    {
        return !myStatus.IsDead;
    }
}
