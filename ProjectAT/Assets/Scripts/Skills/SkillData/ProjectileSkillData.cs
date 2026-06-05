using UnityEngine;
using SkillDataOptionInterfaces;

[CreateAssetMenu(fileName = "New Projectile Skill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkillData : SkillData, IAoESkillData
{
    [Header("Projectile Stats")]
    public GameObject ThrowingObjectPrefab;   // 투사체로 사용할 프리팹.
    public float LandedNoiseRange;

    public virtual float AoERadius => LandedNoiseRange;
}
