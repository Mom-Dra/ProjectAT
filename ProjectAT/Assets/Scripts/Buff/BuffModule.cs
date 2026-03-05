using System.Collections.Generic;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

public class BuffModule : MonoBehaviour
{
    private List<BuffInstance> activeBuffs = new List<BuffInstance>();

    // 임시 Debug 용!
    [SerializeField]
    private BuffData buffData;

    public void AddBuff(BuffData buffData)
    {
        BuffInstance newBuff = new BuffInstance(buffData);
        activeBuffs.Add(newBuff);
        buffData.OnApply(gameObject, newBuff);
    }

    public void Update()
    {
        if (TryGetComponent(out EntityStatus entityStatus))
            Debug.Log(entityStatus.WalkSpeed);
    }

    [ContextMenu("함수 실행하기")]
    private void Foo()
    {
        AddBuff(buffData);
    }
}

// 10, 11
// 여기서 11의 20%가 오르는건지, 10의 20%가 오르는건지..!
// 10%, 20%
