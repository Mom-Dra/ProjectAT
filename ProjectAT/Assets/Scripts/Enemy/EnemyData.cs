using Interactable;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private DownedBody corpsePrefab;

    [SerializeField]
    private float attackRange = 10f;

    [SerializeField]
    private float rotateSpeed = 5f;

    [SerializeField]
    private float aimAngleThreshold = 5f;

    [SerializeField]
    private float searchRadius = 10f;

    [SerializeField]
    private float searchPointWaitTime = 1f;

    [SerializeField]
    private float investigateWaitTime = 2f;

    [SerializeField]
    private float searchNavSampleRadius = 2f;

    [SerializeField]
    private int maxAttempts = 5;

    [SerializeField]
    private float targetInformInterval = 0.2f;

    [Header("�þ�")]
    [SerializeField, Range(0, 360)]
    private float viewAngle;
    [SerializeField]
    private float primaryViewRadius;
    [SerializeField]
    private float secondaryViewRadius;


    [SerializeField]
    private float timeToLostTarget = 3f;
    [SerializeField]
    private float minHideTime = 1.5f;
    [SerializeField]
    private float maxHideTime = 3.5f;

    [SerializeField]
    private float reactionTime = 0.6f;

    [Header("Cover")]
    [SerializeField]
    private bool useCover = true;
    [SerializeField]
    private float coverSearchRadius = 8f;
    [SerializeField]
    private float coverSearchCooldown = 1f;
    [SerializeField]
    private float coverNavSampleRadius = 1.5f;
    [SerializeField, Range(90f, 180f)]
    private float coverOppositeSideAngleThreshold = 105f;

    public DownedBody CorpsePrefab => corpsePrefab;

    public float AttackRange => attackRange;
    public float RotateSpeed => rotateSpeed;
    public float AimAngleThreshold => aimAngleThreshold;
    public float SearchRadius => searchRadius;
    public float SearchPointWaitTime => searchPointWaitTime;
    public float InvestigateWaitTime => investigateWaitTime;
    public float SearchNavSampleRadius => searchNavSampleRadius;
    public int SearchMaxAttempts => maxAttempts;
    public float TargetInformInterval => targetInformInterval;

    public float ViewAngle => viewAngle;
    public float PrimaryViewRadius => primaryViewRadius;
    public float SecondaryViewRadius => secondaryViewRadius;

    public float TimeToLostTarget => timeToLostTarget;
    public float MinHideTime => minHideTime;
    public float MaxHideTime => maxHideTime;

    public float ReactionTime => reactionTime;

    public bool UseCover => useCover;
    public float CoverSearchRadius => coverSearchRadius;
    public float CoverSearchCooldown => coverSearchCooldown;
    public float CoverNavSampleRadius => coverNavSampleRadius;
    public float CoverOppositeSideAngleThreshold => coverOppositeSideAngleThreshold;
}
