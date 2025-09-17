using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WeaponAttackAction", story: "[Weapon] attacks", category: "Action", id: "6f4918a792f416e5bbb323931bf4013f")]
public partial class WeaponAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<Weapon> Weapon;

    protected override Status OnUpdate()
    {
        Weapon.Value.Attack();
        return Status.Success;
    }
}
