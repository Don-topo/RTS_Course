using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsGameObjectNull", story: "Check if the [GameObject] is not null .", category: "Conditions", id: "7030f9471c7225718516fbc0148b6151")]
public partial class IsGameObjectNullCondition : Condition
{
    [SerializeReference] new public BlackboardVariable<GameObject> GameObject;

    public override bool IsTrue()
    {
        return GameObject.Value != null;
    }
}
