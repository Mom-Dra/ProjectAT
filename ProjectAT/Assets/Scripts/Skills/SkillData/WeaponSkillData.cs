using UnityEngine;
using SkillDataOptionInterfaces;

[CreateAssetMenu(fileName = "New Weapon Skill Data", menuName = "Skills/Weapon Skill Data")]
public class WeaponSkillData : SkillData, IWeaponUsingSkillData
{
    [Header("Weapon Skill Option")]
    [SerializeField] private bool requiresAmmo;
    [SerializeField] private int ammoCostPerShot;

    public bool RequiresAmmo => requiresAmmo;
    public int AmmoCostPerShot => ammoCostPerShot;    
}
