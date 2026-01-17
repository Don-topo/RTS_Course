using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopAgent", story: "[Agent] stops moving .", category: "Action/Navigation", id: "bb46080e125592023523f721bf31a5cb")]
public partial class StopAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        if(Agent.Value.TryGetComponent(out NavMeshAgent agent))
        {
            if(agent.TryGetComponent(out Animator animator))
            {
                animator.SetFloat(AnimationConstants.SPEED, 0);
            }
            agent.ResetPath();
            return Status.Success;
        }

        return Status.Failure;
    }
}

