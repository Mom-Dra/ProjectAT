using UnityEngine;


public class PlayerSkillStrategyMap
{
    private ISkill[] skillSlots = new ISkill[5];

    public PlayerSkillStrategyMap()
    {
        InitiateSkillStrategy();
    }

    public void InitiateSkillStrategy() //아마 추후 개발하면서 플레이어 타입을 받아야 할듯?
    {
        skillSlots[0] = new DesignatedFire();
        //그외 스킬 추가로 생성
    }

    public ISkill GetSkillStrategy(int idx)
    {
        if(idx < 0 || idx >= skillSlots.Length)
        {
            Debug.LogError("Invalid skill index");
            return null;
        }
        return skillSlots[idx];
    }
}
