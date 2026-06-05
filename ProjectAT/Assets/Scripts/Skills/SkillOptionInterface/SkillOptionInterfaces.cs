using UnityEngine;

namespace SkillOptionInterfaces
{
    public interface IInventoryCostSkill
    {
        ItemData NeededItemData {get;}
        int NeededItemAmount {get;}
        bool HasEnoughItem();
        bool TryConsumeItem();
    }

    public interface IWeaponUsingSkill
    {
        bool RequeiresAmmo{get;}
        bool HasEnoughAmmo();
    }
}

namespace SkillDataOptionInterfaces
{
    public interface IConsumableSkillData
    {
        ItemData NeededItemData {get;}
        int NeededItemAmount {get;}
    }
    
    public interface IAoESkillData
    {
        float AoERadius {get;}
    }
}