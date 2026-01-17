using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Scriptable Objects/Item Data")]
public class ItemData : ScriptableObject
{
    public string ItemName; // 아이템 이름 (예: Bandage)
    public Sprite Icon;     // 아이콘 이미지
    public int ItemCode;
    public int MaxStack;   // 최대 스택
    [TextArea]
    public string Description; // 설명
}