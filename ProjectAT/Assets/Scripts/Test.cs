using System;
using Unity.Behavior;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    private int num;
    private BehaviorGraphAgent behaviorGraphAgent;

    private void Awake()
    {
        behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
    }

    [ContextMenu("SwitchState")]
    private void SwitchState()
    {
        Debug.Log($"{(TreeTest)num}");
        behaviorGraphAgent.SetVariableValue("state", (TreeTest)num);
        num = (num + 1) % 3;
    }
}
