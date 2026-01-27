using System;
using Unity.Netcode;
using UnityEngine;

public class EntityStatus: MonoBehaviour, IDamageable
{
    public event Action onDeath;

    //References
    [SerializeField] private EntityInitialStatus initStatus;

    public int CurrentHp { get; set; }
    public int MaxHp { get; private set; }
    public float WalkSpeed { get; private set; }
    public float RunSpeed { get; private set; }
    public bool IsDead { get; private set; }
    public float ThrowRange {get; private set;}
    public float MaxViewingDistance {get; private set;}


    private void OnEnable()
    {
        InitStatus();
    }


    private void InitStatus()
    {
        MaxHp = initStatus.MaxHp;
        CurrentHp = MaxHp;
        WalkSpeed = initStatus.WalkSpeed;
        RunSpeed = initStatus.RunSpeed;
        ThrowRange = initStatus.ThrowRange;
        MaxViewingDistance = initStatus.MaxViewingDistance;
        IsDead = false;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{transform.name} TakeDamage: {damage}");

        CurrentHp -= damage;
        
        if (CurrentHp <= 0)
        {
            IsDead = true;
            Die();

            Debug.Log("Dead!");
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
    }
}
