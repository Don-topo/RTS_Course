using System;
using System.Linq;
using Unity.Behavior;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToGatherableSupply", story: "[Agent] moves to [Supply] or nearby supply .", category: "Action/Navigation", id: "d5fe98f6a8981fd5c3ee06407640d4ad")]
public partial class MoveToGatherableSupplyAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GatherableSupply> Supply;
    [SerializeReference] public BlackboardVariable<float> SearchRadius = new(7f);

    private NavMeshAgent agent;
    private LayerMask suppliesMask;
    private SupplySO supplySO;
    private Animator animator;

    protected override Status OnStart()
    {
        suppliesMask = LayerMask.GetMask("Supplies");

        if(!HasValidInputs())
        {
            return Status.Failure;
        }

        agent.TryGetComponent(out animator);

        Vector3 targetPosition = GetTargetPosition();

        agent.SetDestination(targetPosition);
        return Status.Running;
    }


    protected override Status OnUpdate()
    {
        if (animator != null)
        {
            animator.SetFloat(AnimationConstants.SPEED, agent.velocity.magnitude);
        }

        if (agent.pathPending) return Status.Running;

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            return Status.Running;
        }

        if (Supply.Value != null && !Supply.Value.IsBusy && Supply.Value.Amount > 0)
        {
            return Status.Success;
        }
        Collider[] colliders = FindNearbyNotBusyColliders();

        if (colliders.Length > 0)
        {
            Array.Sort(colliders, new ClosestColliderComparer(agent.transform.position));

            Supply.Value = colliders[0].GetComponent<GatherableSupply>();
            agent.SetDestination(GetTargetPosition());
            return Status.Running;
        }

        return Status.Failure;
    }

    protected override void OnEnd()
    {
        if (animator != null)
        {
            animator.SetFloat(AnimationConstants.SPEED, 0);
        }
    }

    private bool HasValidInputs()
    {
        if (!Agent.Value.TryGetComponent(out agent) || Supply.Value == null && supplySO == null)
        {
            return false;
        }

        if (Supply.Value != null)
        {
            supplySO = Supply.Value.Supply;
        }
        else
        {
            Collider[] colliders = FindNearbyNotBusyColliders();
            if (colliders.Length > 0)
            {
                Array.Sort(colliders, new ClosestColliderComparer(agent.transform.position));
                Supply.Value = colliders[0].GetComponent<GatherableSupply>();
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private Collider[] FindNearbyNotBusyColliders()
    {
        return Physics.OverlapSphere(
            agent.transform.position,
            SearchRadius,
            suppliesMask
        ).Where(collider =>
            collider.TryGetComponent(out GatherableSupply supply)
            && !supply.IsBusy
            && supply.Supply.Equals(supplySO)
        ).ToArray();
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition;
        if (Supply.Value.TryGetComponent(out Collider collider))
        {
            targetPosition = collider.ClosestPoint(agent.transform.position);
        }
        else
        {
            targetPosition = Supply.Value.transform.position;
        }

        return targetPosition;
    }
}

