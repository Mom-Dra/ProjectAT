using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField]
    private float attackRange = 10;

    [SerializeField]
    private float rotateSpeed = 5;

    [SerializeField]
    private float searchRadius = 10;

    [SerializeField]
    private float targetInformInterval = 0.2f;

    [Header("½Ã¾ß")]
    [SerializeField, Range(0, 360)]
    private float viewAngle;
    [SerializeField]
    private float primaryViewRadius;
    [SerializeField]
    private float secondaryViewRadius;

    public float AttackRange => attackRange;
    public float RotateSpeed => rotateSpeed;
    public float SearchRadius => searchRadius;
    public float TargetInformInterval => targetInformInterval;

    public float ViewAngle => viewAngle;
    public float PrimaryViewRadius => primaryViewRadius;
    public float SecondaryViewRadius => secondaryViewRadius;
}
