using EntitySkills;
using UnityEngine;

public static class SkillFactory
{
    public static Skill Create(PlayerSkillModule context, SkillData data)
    {
        if (context == null)
        {
            Debug.LogError("SkillFactory: PlayerSkillModule context is null.");
            return null;
        }

        if (data == null)
        {
            Debug.LogError("SkillFactory: SkillData is null.");
            return new DummySkill(context, null);
        }

        return data.SkillId switch
        {
            SkillId.ThrowRock => CreateThrowRock(context, data),
            SkillId.ThrowGrenade => CreateThrowGrenade(context, data),
            SkillId.UseBandage => CreateUseBandage(context, data),
            SkillId.DesignatedFire => CreateDesignatedFire(context, data),

            // 나중에 구현
            // SkillId.SuppressiveFire => new SuppressiveFire(context, data),
            // SkillId.MultiShot => new MultiShot(context, data),

            SkillId.Dummy => new DummySkill(context, data),
            _ => CreateUnsupportedSkill(context, data),
        };
    }

    private static Skill CreateThrowRock(PlayerSkillModule context, SkillData data)
    {
        if (data is not ProjectileSkillData)
            LogDataMismatch(data, nameof(ProjectileSkillData));

        return new ThrowRock(context, data);
    }

    private static Skill CreateThrowGrenade(PlayerSkillModule context, SkillData data)
    {
        if (data is not GrenadeSkillData)
            LogDataMismatch(data, nameof(GrenadeSkillData));

        return new ThrowGrenade(context, data);
    }

    private static Skill CreateUseBandage(PlayerSkillModule context, SkillData data)
    {
        if (data is not ItemCostSkillData)
            LogDataMismatch(data, nameof(ItemCostSkillData));

        return new UseBandage(context, data);
    }

    private static Skill CreateDesignatedFire(PlayerSkillModule context, SkillData data)
    {
        if (data is not WeaponSkillData)
            LogDataMismatch(data, nameof(WeaponSkillData));
        return new DesignatedFire(context, data);
    }

    private static Skill CreateUnsupportedSkill(PlayerSkillModule context, SkillData data)
    {
        Debug.LogWarning($"SkillFactory: Unsupported SkillId '{data.SkillId}' on '{data.name}'. DummySkill will be used.");
        return new DummySkill(context, data);
    }

    private static void LogDataMismatch(SkillData data, string expectedType)
    {
        Debug.LogWarning(
            $"SkillFactory: SkillData '{data.name}' has SkillId '{data.SkillId}' but is not {expectedType}."
        );
    }
}
