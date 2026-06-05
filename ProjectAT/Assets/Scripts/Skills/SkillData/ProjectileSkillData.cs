using UnityEngine;
using SkillDataOptionInterfaces;

[CreateAssetMenu(fileName = "New Projectile Skill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkillData : SkillData, IAoESkillData
{
    [Header("Projectile Stats")]
    [SerializeField] protected GameObject throwingObjectPrefab;   // 투사체로 사용할 프리팹.
    [SerializeField] protected float landedNoiseRange;

    public virtual float AoERadius => LandedNoiseRange;
    public float LandedNoiseRange => landedNoiseRange;
    public GameObject ThrowingObjectPrefab => throwingObjectPrefab;
}
