using System;
using Unity.Netcode;
using UnityEngine;

public class EntityStatus : MonoBehaviour, IDamageable
{
    public event Action onDeath;
    public event Action onRevive;
    public event Action<float> onHealthChanged;

    //References
    [SerializeField] private EntityInitialStatus initStatus;
    private CoverHandler coverHandler;

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
    public EntityInitialStatus InitStatusRef => initStatus;
    public Action<float> OnHealthChanged => onHealthChanged;

    private void Awake()
    {
        coverHandler = GetComponent<CoverHandler>();
    }


    private void OnEnable()
    {
        InitStatus();
    }

    private void InitStatus()
    {
        maxHpStat = new Stat(initStatus.MaxHp);
        walkSpeedStat = new Stat(initStatus.WalkSpeed);
        runSpeedStat = new Stat(initStatus.RunSpeed);
        armorStat = new Stat(initStatus.Armor);

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

    public void TakeDamage(int damage, EntityStatus attacker)
    {
        if (IsDead) return;

        Debug.Log($"{transform.name} TakeDamage: {damage}");

        float finalDamage = damage;

        if (attacker is not null)
        {
            // A. 엄폐 보너스 계산 (공격자의 위치 활용)
            if (coverHandler is not null)
            {
                float coverBonus = coverHandler.GetCoverBonus(attacker.transform);
                finalDamage *= 1f - coverBonus;
            }
        }

        CurrentHp -= Mathf.RoundToInt(finalDamage);
        onHealthChanged?.Invoke(Mathf.Clamp01(Ratio));

        if (CurrentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        CurrentHp += healAmount;
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);

        onHealthChanged?.Invoke(Mathf.Clamp01(Ratio));

        Debug.Log($"{transform.name} Healed: {healAmount}, CurrentHp: {CurrentHp}");
    }

    public void Revive(int reviveHp = 1)
    {
        IsDead = false;
        CurrentHp = Mathf.Min(reviveHp, MaxHp);
        onHealthChanged?.Invoke(Mathf.Clamp01((float)CurrentHp / MaxHp));
        Debug.Log($"{transform.name} Revived! CurrentHp: {CurrentHp}");
        onRevive?.Invoke();
    }

    [ContextMenu("Die")]
    private void Die()
    {
        // Enemy�� ��� ������ ���߰� �״� Animation ���
        // ���⼭ �ٷ� Enemy�� Animator�� ���������� ������?
        Debug.Log("Dead!");

        IsDead = true;
        onDeath?.Invoke();
    }
}
