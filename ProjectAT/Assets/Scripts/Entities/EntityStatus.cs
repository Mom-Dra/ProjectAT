using System;
using Unity.Netcode;
using UnityEngine;

public class EntityStatus: MonoBehaviour, IDamageable
{
    public event Action onDeath;

    //References
    [SerializeField] private EntityInitialStatus initStatus;

    [field: SerializeField] public int CurrentHp { get; set; }
    [field: SerializeField]public int MaxHp { get; private set; }
    [field: SerializeField]public float WalkSpeed { get; private set; }
    [field: SerializeField]public float RunSpeed { get; private set; }
    [field: SerializeField]public bool IsDead { get; private set; }
    [field: SerializeField]public float ThrowRange {get; private set;}
    [field: SerializeField]public float MaxViewingDistance {get; private set;}


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

    public void Heal(int healAmount)
    {
        CurrentHp += healAmount;
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
        Debug.Log($"{transform.name} Healed: {healAmount}, CurrentHp: {CurrentHp}");
    }

    private void Die()
    {
        // Enemy�� ��� ������ ���߰� �״� Animation ���
        // ���⼭ �ٷ� Enemy�� Animator�� ���������� ������?

        onDeath?.Invoke();
    }
}
