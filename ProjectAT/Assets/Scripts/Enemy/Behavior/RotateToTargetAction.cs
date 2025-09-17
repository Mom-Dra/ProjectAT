using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RotateToTarget", story: "[Self] Rotate To [Target] With [RotateSpeed] To [Muzzle] Forward", category: "Action", id: "9c42388f6a0debbe239bf8cc93166f88")]
public partial class RotateToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> RotateSpeed;
    [SerializeReference] public BlackboardVariable<Transform> Muzzle;

    private Vector3 dir;
    private Quaternion targetRotation;

    protected override Status OnStart()
    {
        dir = Target.Value.transform.position - Muzzle.Value.transform.position;
        dir.y = 0f;

        targetRotation = Quaternion.LookRotation(dir);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Self.Value.transform.rotation = Quaternion.Lerp(Self.Value.transform.rotation, targetRotation, RotateSpeed.Value * Time.deltaTime);

        if (Vector3.Angle(Self.Value.transform.forward, dir) < 1f)
            return Status.Success;

        return Status.Running;
    }
}

