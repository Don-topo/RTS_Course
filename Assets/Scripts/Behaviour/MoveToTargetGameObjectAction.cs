using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Target GameObject", story: "[Agent] moves to [TargetGameObject] .", category: "Action/Navigation", id: "5f53d3fd3879df878151d4767b2cc959")]
public partial class MoveToTargetGameObjectAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> TargetGameObject;
    [SerializeReference] public BlackboardVariable<float> MoveThreshold = new(0.25f);

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 lastPosition;

    protected override Status OnStart()
    {
        if (!Agent.Value.TryGetComponent(out agent) || TargetGameObject.Value == null)
        {
            return Status.Failure;
        }

        Agent.Value.TryGetComponent(out animator);

        Vector3 targetPosition = GetTargetPosition();

        if (Vector3.Distance(agent.transform.position, targetPosition) <= agent.stoppingDistance)
        {
            return Status.Success;
        }

        agent.SetDestination(targetPosition);
        lastPosition = targetPosition;
        return Status.Running;
    }


    protected override Status OnUpdate()
    {
        if (animator != null)
        {
            animator.SetFloat(AnimationConstants.SPEED, agent.velocity.magnitude);
        }
        if (agent.pathPending) return Status.Running;

        Vector3 targetPosition = GetTargetPosition();
        if(Vector3.Distance(targetPosition, lastPosition) >= MoveThreshold)
        {
            agent.SetDestination(targetPosition);
            lastPosition = agent.destination;
            return Status.Running;
        }
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (animator != null)
        {
            animator.SetFloat(AnimationConstants.SPEED, 0);
        }
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 targetPosition;
        if (TargetGameObject.Value.TryGetComponent(out Collider collider))
        {
            targetPosition = collider.ClosestPoint(agent.transform.position);
        }
        else
        {
            targetPosition = TargetGameObject.Value.transform.position;
        }

        return targetPosition;
    }
}

