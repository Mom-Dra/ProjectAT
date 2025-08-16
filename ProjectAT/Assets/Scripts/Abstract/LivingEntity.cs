using System;
using Unity.Netcode;
using UnityEngine;

public abstract class LivingEntity : NetworkBehaviour, IDamageable
{
    private NetworkVariable<int> health = new NetworkVariable<int>(100);
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);

    public event Action onDeath;

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    // OnlyServer
    public void TakeDamage(int damage)
    {
        health.Value -= damage;

        if (health.Value <= 0 && !isDead.Value)
        {
            Die();
        }
    }

    protected virtual void OnHealthChanged(int previousHealth, int currentHealth)
    {
        // Update UI, Effect, Sounds...

    }

    private void OnIsDeadChanged(bool previousIsDead, bool currentIsDead)
    {
        if (currentIsDead)
        {
            onDeath?.Invoke();
        }
    }

    private void DestoryNetworkObject()
    {
        if (!NetworkObject.IsSpawned) return;

        NetworkObject.Despawn(true);
    }

    // OnlyServer
    private void Die()
    {
        isDead.Value = true;
        DestoryNetworkObject();
    }
}
