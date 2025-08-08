using Unity.Netcode;
using UnityEngine;

public class EntityStatus: NetworkBehaviour
{
    [SerializeField] private int currentHp;
    [SerializeField] private int maxHp;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public int CurrentHp { get; }
    public int MaxHp { get; }
    public float WalkSpeed { get { return walkSpeed; } }
    public float RunSpeed { get { return runSpeed; } }

}
