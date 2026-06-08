using UnityEngine;
using SkillDataOptionInterfaces;
using SkillOptionInterfaces;

[CreateAssetMenu(fileName = "New DurationAreaWeaponSkillData", menuName = "Skills/Duration Area Weapon Skill Data")]
public class DurationAreaWeaponSkillData : WeaponSkillData, IAoESkillData
{
    [Header("Suppressive Fire")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float tickInterval = 0.1f;
    [SerializeField] private float aoeRadius = 10f;

    public float Duration => duration;
    public float TickInterval => tickInterval;
    public float AoERadius => aoeRadius;
}
