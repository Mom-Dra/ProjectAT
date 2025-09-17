using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Rendering;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SearchTarget", story: "[Self] Search [Target] And Return [Success] With [radius] And Return [canAttack]", category: "Action", id: "3e262c0605449df31f554223664ffb13")]
public partial class SearchTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> Success;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<bool> CanAttack;
    //private float time;
    //private const float CHECKINTERVAL = 0.2f;

    private Collider[] colliders = new Collider[4];

    protected override Status OnStart()
    {
        Debug.Log("SearchTargetAction Start");

        // radius ������ �� ��!
        int count = Physics.OverlapSphereNonAlloc(Self.Value.transform.position, Radius.Value, colliders, LayerMask.GetMask("Player"));
        if (count >= 1)
        {
            Transform target = colliders[0].transform;

            Vector3 dir = target.position - Self.Value.transform.position;
            float distance = Vector3.Distance(Self.Value.transform.position, target.position);

            // isTargetDetedted = true; -> 타켓을 탐지했는지만x
            Target.Value = target.gameObject;
            Success.Value = true;
            CanAttack.Value = false;

            if (!Physics.Raycast(Self.Value.transform.position, dir, distance, LayerMask.GetMask("Obstacle")))
            {
                // canAttack -> 공격할 수 있는지!
                Target.Value = target.gameObject;
                Success.Value = true;
                CanAttack.Value = true;
            }
        }
        else
        {
            Success.Value = false;
            CanAttack.Value = false;
        }

        return Status.Success;
    }
}

