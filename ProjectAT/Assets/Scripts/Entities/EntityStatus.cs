using Unity.Netcode;
using UnityEngine;

public class EntityStatus: MonoBehaviour, IDamageable
{
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
        CurrentHp -= damage;
        Debug.Log("Hit");
        if (CurrentHp <= 0)
        {
            IsDead = true;
            Debug.Log("Dead!");    
        }
    }
}
