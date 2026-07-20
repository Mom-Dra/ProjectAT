using UnityEngine;
using SkillDataOptionInterfaces;


[CreateAssetMenu(fileName = "New Grenade Skill", menuName = "Skills/GrenadeSkillData")]
public class GrenadeSkillData : ProjectileSkillData, IConsumableSkillData
{
    [Header("Grenade Specific Stats")]
    [SerializeField] protected ItemData requiredGrenadeItem;
    [SerializeField] protected int requiredGrenadeItemAmount = 1;
    [SerializeField] protected float explosionRadius;
    [SerializeField] protected float fuseTime;
    [SerializeField] protected float explosionNoiseRadius;
    [SerializeField] protected BuffData[] buffsToApplyOnExplosion;

    public override float AoERadius => explosionRadius;
    public float ExplosionRadius => explosionRadius;
    public float FuseTime => fuseTime;
    public float ExplosionNoiseRadius => explosionNoiseRadius;
    public ItemData NeededItemData => requiredGrenadeItem;
    public int NeededItemAmount => requiredGrenadeItemAmount;
    public BuffData[] BuffsToApplyOnExplosion => buffsToApplyOnExplosion;
}
