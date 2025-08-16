using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckTarget", story: "Compare values of [CurrentDistance] and [ChaseDistance]", category: "Conditions", id: "20ad4f09f8a8d1fa84d5d89868e214cc")]
public partial class CheckTargetCondition : Condition
{
    [SerializeReference] public BlackboardVariable<float> CurrentDistance;
    [SerializeReference] public BlackboardVariable<float> ChaseDistance;

    public override bool IsTrue()
    {
        if(CurrentDistance.Value <= ChaseDistance.Value)
        {
            return true;
        }

        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
