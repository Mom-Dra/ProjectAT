using System;
using Unity.Netcode;
using UnityEngine;

public class EntityStatus: MonoBehaviour, IDamageable
{
    public event Action onDeath;
    public event Action onRevive;
    public event Action<float> onHealthChanged;

    //References
    [SerializeField] private EntityInitialStatus initStatus;

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

    public void TakeDamage(int damage)
    {
        Debug.Log($"{transform.name} TakeDamage: {damage}");

        // Armor 수치에 따른 데미지 감소 로직..!
        // 100 데메지 100, 300   

        CurrentHp -= damage;
        onHealthChanged?.Invoke(Mathf.Clamp01(Ratio));

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

    private void Die()
    {
        // Enemy�� ��� ������ ���߰� �״� Animation ���
        // ���⼭ �ٷ� Enemy�� Animator�� ���������� ������?
        IsDead = true;
        onDeath?.Invoke();
    }
}
