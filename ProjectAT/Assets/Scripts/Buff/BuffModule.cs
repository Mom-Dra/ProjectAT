using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffModule : MonoBehaviour
{
    [Header("Available Buffs")]
    [SerializeField] private BuffData[] availableBuffs;

    private readonly List<BuffInstance> activeBuffs = new List<BuffInstance>();
    private readonly Dictionary<string, BuffData> buffDictionary = new Dictionary<string, BuffData>();

    public IReadOnlyList<BuffInstance> ActiveBuffs => activeBuffs;

    public event Action<BuffInstance> OnBuffAdded;
    public event Action<BuffInstance> OnBuffRemoved;
    public event Action<BuffInstance> OnBuffRefreshed;
    public event Action OnBuffsChanged;

    private void Awake()
    {
        InitiateBuffs();
    }

    private void Update()
    {
        UpdateBuff();
    }

    public bool AddBuff(BuffData buffData)
    {
        if (!CanUseBuffData(buffData))
        {
            return false;
        }

        BuffInstance activeBuff = FindActiveBuff(buffData.BuffName);

        if (activeBuff != null)
        {
            activeBuff.Refresh();
            OnBuffRefreshed?.Invoke(activeBuff);
            OnBuffsChanged?.Invoke();
            return true;
        }

        BuffInstance newBuff = new BuffInstance(buffData);
        activeBuffs.Add(newBuff);
        buffData.OnApply(gameObject, newBuff);

        OnBuffAdded?.Invoke(newBuff);
        OnBuffsChanged?.Invoke();
        return true;
    }

    public bool AddBuff(string buffName)
    {
        BuffData buffData = GetBuffData(buffName);
        return buffData != null && AddBuff(buffData);
    }

    public bool Remove(BuffData buffData)
    {
        if (!CanUseBuffData(buffData))
        {
            return false;
        }

        return Remove(buffData.BuffName);
    }

    public bool Remove(string buffName)
    {
        int index = FindActiveBuffIndex(buffName);

        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public void RemoveBuff(string buffName)
    {
        Remove(buffName);
    }

    private void UpdateBuff()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; --i)
        {
            BuffInstance buffInstance = activeBuffs[i];

            UpdateBuffTick(buffInstance);

            if (buffInstance.IsPermanent)
            {
                continue;
            }

            buffInstance.RemainingTime -= Time.deltaTime;

            if (buffInstance.RemainingTime <= 0f)
            {
                RemoveAt(i);
            }
        }
    }

    private void UpdateBuffTick(BuffInstance buffInstance)
    {
        if (buffInstance.TickInterval <= 0f)
        {
            buffInstance.BuffData.OnUpdate(gameObject, buffInstance);
            return;
        }

        buffInstance.NextTickTime -= Time.deltaTime;

        while (buffInstance.NextTickTime <= 0f)
        {
            buffInstance.BuffData.OnUpdate(gameObject, buffInstance);
            buffInstance.NextTickTime += buffInstance.TickInterval;
        }
    }

    private void RemoveAt(int index)
    {
        BuffInstance buffInstance = activeBuffs[index];

        buffInstance.BuffData.OnRemove(gameObject, buffInstance);
        activeBuffs.RemoveAt(index);

        OnBuffRemoved?.Invoke(buffInstance);
        OnBuffsChanged?.Invoke();
    }

    private void InitiateBuffs()
    {
        buffDictionary.Clear();

        if (availableBuffs == null)
        {
            return;
        }

        for (int i = 0; i < availableBuffs.Length; ++i)
        {
            BuffData buffData = availableBuffs[i];

            if (!CanUseBuffData(buffData))
            {
                continue;
            }

            if (buffDictionary.ContainsKey(buffData.BuffName))
            {
                Debug.LogWarning($"Duplicate buff name registered: {buffData.BuffName}", this);
                continue;
            }

            buffDictionary.Add(buffData.BuffName, buffData);
        }
    }

    private BuffInstance FindActiveBuff(string buffName)
    {
        int index = FindActiveBuffIndex(buffName);
        return index >= 0 ? activeBuffs[index] : null;
    }

    private int FindActiveBuffIndex(string buffName)
    {
        if (string.IsNullOrWhiteSpace(buffName))
        {
            return -1;
        }

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            if (activeBuffs[i].BuffName == buffName)
            {
                return i;
            }
        }

        return -1;
    }

    private BuffData GetBuffData(string buffName)
    {
        if (string.IsNullOrWhiteSpace(buffName))
        {
            Debug.LogWarning("Buff name is empty.", this);
            return null;
        }

        if (buffDictionary.TryGetValue(buffName, out BuffData buffData))
        {
            return buffData;
        }

        Debug.LogWarning($"Buff with name {buffName} not found!", this);
        return null;
    }

    private bool CanUseBuffData(BuffData buffData)
    {
        if (buffData == null)
        {
            Debug.LogWarning("BuffData is null.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(buffData.BuffName))
        {
            Debug.LogWarning($"BuffData {buffData.name} has an empty BuffName.", this);
            return false;
        }

        return true;
    }
}
