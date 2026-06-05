using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Skills/Consumable Skill Data")]
public class ConsumableSkillData : SkillData
{
    public ItemData NeededItemData;
    public int NeededItemAmount = 1;
}
