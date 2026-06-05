using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private Dictionary<ItemData, int> itemContainer = new Dictionary<ItemData, int>();    // Key: 아이템 데이터, Value: 소지 개수
    [SerializeField] private ItemData[] debugItemList;

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
        if (itemContainer.ContainsKey(item))
        {
            amount = Mathf.Min(amount, item.MaxStack - itemContainer[item]);
            itemContainer[item] += amount;
        }
        else
        {
            amount = Mathf.Min(amount, item.MaxStack);
            itemContainer.Add(item, amount);
        }
        
        Debug.Log($"{item.ItemName} 획득! 현재 개수: {itemContainer[item]}");
        // 여기에 UI 업데이트 코드를 추가하면 됩니다.
    }

    // 아이템 개수 확인 (스킬 사용 조건 체크용)
    public int GetItemCount(ItemData item)
    {
        if (itemContainer.ContainsKey(item))
        {
            return itemContainer[item];
        }

        Debug.Log($"{item.ItemName} 아이템이 인벤토리에 없습니다.");
        return 0; // 없으면 0개
    }

    // 아이템 사용 (소모)
    public bool TryUseItem(ItemData item, int amount = 1)
    {
        // 1. 아이템이 있는지, 개수가 충분한지 확인
        if (itemContainer.ContainsKey(item) && itemContainer[item] >= amount)
        {
            // 2. 개수 차감
            itemContainer[item] -= amount;
            Debug.Log($"{item.ItemName} 사용함. 남은 개수: {itemContainer[item]}");

            // 3. 0개가 되면 목록에서 지울지, 0으로 남길지는 선택 (여기선 0으로 남김)
            if(itemContainer[item] <= 0)
            {
                // itemContainer.Remove(item); // 아예 지우고 싶다면 주석 해제
            }
            
            return true; // 사용 성공
        }

        Debug.Log("아이템이 부족합니다!");
        return false; // 사용 실패
    }
}