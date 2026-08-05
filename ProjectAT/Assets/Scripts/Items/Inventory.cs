using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    private Dictionary<ItemData, int> itemContainer = new Dictionary<ItemData, int>();    // Key: 아이템 데이터, Value: 소지 개수
    [SerializeField] private ItemData[] debugItemList;

    public event Action<ItemData, int> OnItemCountChanged; //아이템 개수 변경 시 이벤트. 변경된 아이템과 업데이트된 소지 갯수를 전달

    private void Start()
    {
        // 디버그용 아이템 추가
        foreach(var item in debugItemList)
        {
            AddItem(item, 2); // 각 아이템 2개씩 추가
        }
    }
    
    // 아이템 획득 메서드
    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
        {
            return;
        }

        int previousCount = itemContainer.TryGetValue(item, out int storedCount) ? storedCount : 0;
        int newCount = Mathf.Min(previousCount + amount, item.MaxStack);

        if (newCount == previousCount)
        {
            return;
        }

        itemContainer[item] = newCount;

        Debug.Log($"{item.ItemName} 획득! 현재 개수: {newCount}");
        OnItemCountChanged?.Invoke(item, newCount);
    }

    // 아이템 개수 확인 (스킬 사용 조건 체크용)
    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;
        return itemContainer.TryGetValue(item, out int count) ? count : 0;
    }

    // 아이템 사용 (소모)
    public bool TryUseItem(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        int currentCount = GetItemCount(item);

        if (currentCount < amount)
        {
            Debug.Log("아이템이 부족합니다!");
            return false;
        }

        int newCount = currentCount - amount;
        itemContainer[item] = newCount;

        Debug.Log($"{item.ItemName} 사용함. 남은 개수: {newCount}");
        OnItemCountChanged?.Invoke(item, newCount);

        return true;
    }
}