using System;
using Unity.Netcode;
using UnityEngine;

public abstract class LivingEntity : MonoBehaviour, IDamageable
{
    private int health = 100;
    private bool isDead = false;

    public event Action onDeath;

    // OnlyServer
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0 && !isDead)
        {
            Die();
        }
    }
    
    protected virtual void OnHealthChanged(int previousHealth, int currentHealth)
    {
        // Update UI, Effect, Sounds...

    }

    // OnlyServer
    private void Die()
    {
        isDead = true;
        onDeath?.Invoke();
    }
}
