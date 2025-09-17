using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseTargetPosition", story: "[Self] Chase [Target] Position", category: "Action", id: "be255e8cce5ad785884659d53676c731")]
public partial class ChaseTargetPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private NavMeshAgent navMeshAgent;

    protected override Status OnStart()
    {
        navMeshAgent = Self.Value.GetComponent<NavMeshAgent>();

        return Status.Running;
    }

    //protected override Status OnUpdate()
    //{

    //}
}

