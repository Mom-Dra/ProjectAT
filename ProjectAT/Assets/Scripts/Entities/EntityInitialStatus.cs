using UnityEngine;

[CreateAssetMenu(fileName = "Entity Initial Status", menuName = "Initialize/EntityStatus")]
public class EntityInitialStatus : ScriptableObject
{
    [SerializeField] private int maxHp;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float throwRange = 7.0f;
    [SerializeField] private float maxViewingDistance = 20.0f;

    public int MaxHp { get { return maxHp; } }
    public float WalkSpeed { get { return walkSpeed; } }
    public float RunSpeed { get { return runSpeed; } }
    public float ThrowRange { get { return throwRange; } }
    public float MaxViewingDistance { get { return maxViewingDistance; } }
}
