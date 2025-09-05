using System;
using Unity.Behavior;
using Unity.Netcode;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.AI;

public class Test : NetworkBehaviour
{
    [Rpc(SendTo.ClientsAndHost)]
    public void HahaRpc()
    {
        Debug.Log("haha");
    }
}
