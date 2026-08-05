using System;
using Unity.Netcode;
using UnityEngine;

public class EntityStatus : MonoBehaviour, IDamageable
{
    public event Action onDeath;
    public event Action<float> onHealthChanged;
    public event Action onLowHealthWarning;
    public event Action onLowHealthWarningEnd;

    //References
    [Header("References")]
    [SerializeField] private EntityInitialStatus initStatus;
    [Header("Status Setting")]
    [SerializeField] private float lowHealthThreshold = 0.35f;

    private Stat maxHpStat;
    private Stat walkSpeedStat;
    private Stat runSpeedStat;
    private Stat armorStat;

    public int CurrentHp { get; private set; }
    public int MaxHp => (int)maxHpStat.Value;
    public float WalkSpeed => walkSpeedStat.Value;
    public float RunSpeed => runSpeedStat.Value;
    public float Armor => armorStat.Value;
    public bool IsDead { get; private set; }
    public float ThrowRange { get; private set; }
    public float MaxViewingDistance { get; private set; }

    public float Ratio => (float)CurrentHp / MaxHp;
    public bool IsLowHealth => Ratio <= lowHealthThreshold;
    public EntityInitialStatus InitStatusRef => initStatus;

    private void Awake()
    {
        InitStatus();
    }

    private void OnEnable()
    {
        InitStatus();
    }

    private void Update()
    {
        Debug.Log("Update");
        Debug.Log($"{gameObject.name} walkSpeedStat: {walkSpeedStat.Value}");
    }

    private void InitStatus()
    {
        maxHpStat = new Stat(initStatus.MaxHp, 1f);
        walkSpeedStat = new Stat(initStatus.WalkSpeed, 0.1f);
        runSpeedStat = new Stat(initStatus.RunSpeed, 0.1f);
        armorStat = new Stat(initStatus.Armor, 0f);

        CurrentHp = initStatus.MaxHp;
        ThrowRange = initStatus.ThrowRange;
        MaxViewingDistance = initStatus.MaxViewingDistance;
        IsDead = false;
    }

    public Stat GetStat(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHP => maxHpStat,
            StatType.MoveSpeed => walkSpeedStat,
            StatType.AttackPower => runSpeedStat,
            _ => null
        };
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        float finalDamage = damage; //NOTE : 나중에 방어력 계산식 넣어야함.
        HpChange(-Mathf.RoundToInt(finalDamage));

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        HpChange(healAmount);
    }

    private void HpChange(int changeAmount)
    {
        int newHp = Mathf.Clamp(CurrentHp + changeAmount, 0, MaxHp);
        float newRatio = (float)newHp / MaxHp;

        HandleLowHealthWarning(newRatio);

        CurrentHp = newHp;
        
        onHealthChanged?.Invoke(Mathf.Clamp01(newRatio));
    }

    private void HandleLowHealthWarning(float newHealthRatio)
    {
        bool isNowLowHealth = newHealthRatio <= lowHealthThreshold;

        if(IsLowHealth != isNowLowHealth)
        {
            if(isNowLowHealth)
            {
                onLowHealthWarning?.Invoke();
            }
            else
            {
                onLowHealthWarningEnd?.Invoke();
            }
        }
    }

    [ContextMenu("Die")]
    private void Die()
    {
        Debug.Log("Dead!");

        IsDead = true;
        onDeath?.Invoke();
    }
}
