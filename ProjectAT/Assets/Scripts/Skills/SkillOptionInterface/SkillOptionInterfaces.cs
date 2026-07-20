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
        bool RequiresAmmo{get;}
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

    public interface IWeaponUsingSkillData
    {
        bool RequiresAmmo {get;}
        int AmmoCostPerShot {get;}
    }
    
    public interface IAoESkillData
    {
        float AoERadius { get; }
        float AoELength { get; } 
    }
}