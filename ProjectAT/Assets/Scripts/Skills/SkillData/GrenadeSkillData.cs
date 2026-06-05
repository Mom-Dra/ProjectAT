using UnityEngine;


[CreateAssetMenu(fileName = "New Grenade Skill", menuName = "Skills/GrenadeSkillData")]
public class GrenadeSkillData : ProjectileSkillData
{
    [Header("Grenade Specific Stats")]
    public ItemData requiredGrenadeItem;
    public float ExplosionRadius;
    public float FuseTime;
    public float ExplosionNoiseRadius;

    public override float AoERadius => ExplosionRadius;
}
