using System.Collections.Generic;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

public class BuffModule : MonoBehaviour
{
    private List<BuffInstance> activeBuffs = new List<BuffInstance>();

    public void AddBuff(BuffData buffData)
    {
        BuffInstance newBuff = new BuffInstance(buffData);
        activeBuffs.Add(newBuff);
        buffData.OnApply(gameObject, newBuff);
    }
}
