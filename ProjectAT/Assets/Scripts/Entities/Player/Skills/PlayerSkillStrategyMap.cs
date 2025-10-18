using UnityEngine;

public class PlayerSkillStrategyMap
{
    public ISkill NowActiveSkill { get; private set; }
    private ISkill[] skillSlots = new ISkill[5];

    public void InitiateSkillStrategy(PlayerController context) //아마 추후 개발하면서 플레이어 타입을 받아야 할듯?
    {
        skillSlots[0] = new DesignatedFire(context);
        //그외 스킬 추가로 생성
        NowActiveSkill = skillSlots[0]; //초기 활성 스킬 설정
    }

    public void SetActiveSkill(int index)
    {
        if(index <0 || index >= skillSlots.Length)
        {
            Debug.LogError("Invalid skill slot index: " + index);
            return;
        }
        
        NowActiveSkill = skillSlots[index];
    }
}
