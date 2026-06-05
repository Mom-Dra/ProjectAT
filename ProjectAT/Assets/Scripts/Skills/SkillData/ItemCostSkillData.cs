using UnityEngine;
using SkillDataOptionInterfaces;

[CreateAssetMenu(fileName = "New Skill Data", menuName = "Skills/Item Cost Skill Data")]
public class ItemCostSkillData : SkillData, IConsumableSkillData
{
    [Header("Consumable Skill Specific Stats")]
    [SerializeField] protected ItemData neededItemData;
    [SerializeField] protected int neededItemAmount = 1;

    public ItemData NeededItemData => neededItemData;
    public int NeededItemAmount => neededItemAmount;
}
