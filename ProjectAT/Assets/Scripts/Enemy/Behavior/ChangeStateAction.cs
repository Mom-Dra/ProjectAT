using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChangeState", story: "Change [Enemy] [State]", category: "Action", id: "3bbe2d20495e20b8932e5840c609c81e")]
public partial class ChangeStateAction : Action
{
    [SerializeReference] public BlackboardVariable<Enemy> Enemy;
    [SerializeReference] public BlackboardVariable<Enemy_State> State;

    protected override Status OnStart()
    {
        //Enemy.Value.ChangeState(State.Value);

        return Status.Running;
    }
}

