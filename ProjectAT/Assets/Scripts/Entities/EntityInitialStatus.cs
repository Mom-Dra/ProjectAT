using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Entity Initial Status", menuName = "Initialize/EntityStatus")]
public class EntityInitialStatus : ScriptableObject
{
    [SerializeField] private int maxHp;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float armor;
    [SerializeField] private float throwRange = 7.0f;
    [SerializeField] private float maxViewingDistance = 20.0f;

    public int MaxHp => maxHp;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float Armor => armor;
    public float ThrowRange => throwRange;
    public float MaxViewingDistance => maxViewingDistance;
}
