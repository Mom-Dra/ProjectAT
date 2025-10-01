using Unity.Netcode;
using UnityEngine;

public class EntityStatus: MonoBehaviour
{
    //References
    [SerializeField] private WeaponStatus myWeapon;
    [SerializeField] private EntityInitialStatus initStatus;

    public int CurrentHp { get; set; }
    public float CurrentSpeed { get; set; }
    public int MaxHp { get; private set; }
    public float WalkSpeed { get; private set; }
    public float RunSpeed { get; private set; }
    public bool IsDead { get; private set; }
    public float CurrAttackCoolTime { get; private set; }
    public float MaxAttackCoolTime { get; private set; }


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
        IsDead = false;

        myWeapon = transform.GetChild(1).GetComponent<WeaponStatus>();
        if (myWeapon)
        {
            MaxAttackCoolTime = 2.0f;
        }
    }

    public void Update()
    {
        if (CurrAttackCoolTime > 0.0f) 
        {
            CurrAttackCoolTime -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        Debug.Log("Hit");
        if (CurrentHp <= 0)
        {
            IsDead = true;
            Debug.Log("Dead!");    
        }
    }

    public bool CanFire()
    {
        return CurrAttackCoolTime <= 0;
    }

    public void ResetAttackCoolTime()
    {
        Debug.Log("ResetCoolTime");
        CurrAttackCoolTime = MaxAttackCoolTime;
    }
}
