using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IdleAction", story: "[Self] stops", category: "Action", id: "67cfb604d7480acfc9916744c6247c6a")]
public partial class IdleAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        Debug.Log("IdleAction: OnStart");

        if (Self.Value.TryGetComponent(out NavMeshAgent navMeshAgent) && navMeshAgent.isOnNavMesh)
        {
            Debug.Log($"{Self.Name}, navMeshAgent.ResetPath()");
            navMeshAgent.ResetPath();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Debug.Log("IdleAction: OnUpdate");
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

