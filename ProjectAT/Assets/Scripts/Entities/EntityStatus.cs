using Unity.Netcode;
using UnityEngine;

public class EntityStatus: NetworkBehaviour
{
    //References
    [SerializeField] private WeaponStatus myWeapon;
    [SerializeField] private EntityInitialStatus initStatus;

    //NetworkVariables
    public NetworkVariable<int> CurrentHp = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> MaxHp = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> WalkSpeed = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> RunSpeed = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> IsDead = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> CurrAttackCoolTime = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> MaxAttackCoolTime = new(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitStatus();
        }
    }

    private void InitStatus()
    {
        MaxHp.Value = initStatus.MaxHp; 
        CurrentHp.Value = MaxHp.Value;
        WalkSpeed.Value = initStatus.WalkSpeed;
        RunSpeed.Value = initStatus.RunSpeed;
        IsDead.Value = false;

        myWeapon = transform.GetChild(1).GetComponent<WeaponStatus>();
        if (myWeapon)
        {
            MaxAttackCoolTime.Value = 2.0f;
        }
    }

    public void Update()
    {
        if(IsServer && CurrAttackCoolTime.Value >0.0f)
        {
            CurrAttackCoolTime.Value -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHp.Value -= damage;
        Debug.Log("Hit");
        if (CurrentHp.Value <= 0)
        {
            IsDead.Value = true;
            Debug.Log("Dead!");    
        }
    }

    public bool CanFire()
    {
        return CurrAttackCoolTime.Value <= 0;
    }

    public void ResetAttackCoolTime()
    {
        Debug.Log("ResetCoolTime");
        CurrAttackCoolTime.Value = MaxAttackCoolTime.Value;
    }
}
