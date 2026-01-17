using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Pick Closest Point On Collider", story: "Set [TargetLocation] to the closest point to [Target] on [Collider] .", category: "Action", id: "2d3da2e85d5df16c08741ec6a5a17522")]
public partial class PickClosestPointOnColliderAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Collider;

    protected override Status OnStart()
    {
        if(Target.Value == null || Collider.Value == null || !Collider.Value.TryGetComponent(out Collider collider))
        {
            return Status.Failure;
        }

        TargetLocation.Value = collider.ClosestPoint(Target.Value.transform.position);

        return Status.Success;
    }
}

