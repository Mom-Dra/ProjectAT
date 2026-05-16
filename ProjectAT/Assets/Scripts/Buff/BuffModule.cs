using System.Collections.Generic;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

public class BuffModule : MonoBehaviour
{
    private List<BuffInstance> activeBuffs = new List<BuffInstance>();
    private Dictionary<string, BuffData> buffDictionary = new Dictionary<string, BuffData>(); // 이 Dictionary는 어딘가의 싱글톤 같은걸로 두는게 나을지도. 아니면 팩토리 라던가.
    [Header("Available Buffs")]
    [SerializeField] private BuffData[] availableBuffs;

    private void Awake()
    {
        InitiateBuffs();
    }

    public void AddBuff(BuffData buffData) //버프의 중복 검사는 어떻게 처리할 생각?
    {
        BuffInstance newBuff = new BuffInstance(buffData);
        activeBuffs.Add(newBuff);
        buffData.OnApply(gameObject, newBuff);
    }

    public void AddBuff(string buffName)
    {
        BuffData buffData = GetBuffData(buffName);
        if (buffData != null)
        {
            AddBuff(buffData);
        }
    }

    public void RemoveBuff(string buffName) //제미나이생성 Remove함수 by DualDura
    {
        BuffInstance buffInstance = activeBuffs.Find(buff => buff.BuffData.buffName == buffName);

        if (buffInstance != null)
        {
            buffInstance.BuffData.OnRemove(gameObject, buffInstance);
            activeBuffs.Remove(buffInstance);
        }
    }

    private void InitiateBuffs()
    {
        for(int i = 0; i < availableBuffs.Length; i++)
        {
            buffDictionary.Add(availableBuffs[i].buffName, availableBuffs[i]);
        }
    }
    private BuffData GetBuffData(string buffName) //싱글톤 같은 곳으로 옮겨야할지도?
    {
        if (buffDictionary.TryGetValue(buffName, out BuffData buffData))
        {
            return buffData;
        }
        Debug.LogWarning($"Buff with name {buffName} not found!");
        return null;
    }
}
