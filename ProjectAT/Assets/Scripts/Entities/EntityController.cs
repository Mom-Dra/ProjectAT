using Unity.Netcode;
using UnityEngine;

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
            myStatus.TakeDamage(damage);
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
        return !myStatus.IsDead.Value;
    }
}
