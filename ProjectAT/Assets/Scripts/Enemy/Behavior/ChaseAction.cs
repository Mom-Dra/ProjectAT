using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.InputSystem.Android;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "[Self] Navigate To [Target]", category: "Action", id: "02997a8101c32229707986b5bbd675c3")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private NavMeshAgent agent;

    protected override Status OnStart()
    {
        agent = Self.Value.GetComponent<NavMeshAgent>();
        //agent.speed = 5f;
        agent.SetDestination(Target.Value.transform.position);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }
}

