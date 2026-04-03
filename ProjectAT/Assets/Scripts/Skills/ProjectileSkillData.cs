using UnityEngine;

public class ProjectileSkillData : SkillData
{
    [Header("Projectile Stats")]
    public GameObject ThrowingObjectPrefab;   // 투사체로 사용할 프리팹.
    public float ExplosionRadius;
    public float FuseTime;   
}
