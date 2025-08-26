using UnityEngine;

[CreateAssetMenu(fileName = "Entity Initial Status", menuName = "Initialize/EntityStatus")]
public class EntityInitialStatus : ScriptableObject
{
    [SerializeField] private int maxHp;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    public int MaxHp { get { return maxHp; } }
    public float WalkSpeed { get { return walkSpeed; } }
    public float RunSpeed { get { return runSpeed; } }
}
