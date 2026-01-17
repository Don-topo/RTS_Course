using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using GameDevTV.RTS.Units;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GatherSupplies", story: "[Unit] gathers [Amount] supplies from [GatherableSupplies] .", category: "Action/Units", id: "d46fb05aab4f9ed18791cbc1f0552036")]
public partial class GatherSuppliesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Unit;
    [SerializeReference] public BlackboardVariable<int> Amount;
    [SerializeReference] public BlackboardVariable<GatherableSupply> GatherableSupplies;
    [SerializeReference] public BlackboardVariable<SupplySO> SupplySO;
    [SerializeReference] public BlackboardVariable<GameObject> HeldSupply;

    private float enterTime;
    private Animator animator;
    private ParticleSystem particleSystem;

    protected override Status OnStart()
    {
        if(GatherableSupplies.Value == null)
        {
            return Status.Failure;
        }

        enterTime = Time.time;

        if(Unit.Value.TryGetComponent(out animator))
        {
            animator.SetBool(AnimationConstants.IS_GATHERING, true);
        }

        particleSystem = Unit.Value.GetComponentInChildren<ParticleSystem>(true);
        if(particleSystem != null)
        {
            particleSystem.gameObject.SetActive(true);
        }
        GatherableSupplies.Value.BeginGather();
        SupplySO.Value = GatherableSupplies.Value.Supply;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Quaternion lookRotation = Quaternion.LookRotation(
            (GatherableSupplies.Value.transform.position - Unit.Value.transform.position).normalized
        );
        lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        Unit.Value.transform.rotation = lookRotation;

        if(GatherableSupplies.Value.Supply.BaseGatherTime + enterTime <= Time.time)
        {            
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if(animator != null)
        {
            animator.SetBool(AnimationConstants.IS_GATHERING, false);
        }

        if(particleSystem != null)
        {
            particleSystem.gameObject.SetActive(false);
        }

        if (GatherableSupplies.Value == null) return;

        if (CurrentStatus == Status.Success)
        {
            Amount.Value = GatherableSupplies.Value.EndGather();
            GameObject heldModel = GameObject.Instantiate(GatherableSupplies.Value.HeldPrefab, Unit.Value.transform, false);
            heldModel.transform.localPosition = new Vector3(0, 1.25f, 0.32f);
            HeldSupply.Value = heldModel;

            if(Unit.Value.TryGetComponent(out HoldGunIK holdGunIK))
            {
                holdGunIK.leftElbowIKTarget = heldModel.transform.Find("LeftElbowTarget");
                holdGunIK.leftHandIKTarget = heldModel.transform.Find("LeftHandTarget");
                holdGunIK.rightElbowIKTarget = heldModel.transform.Find("RightElbowTarget");
                holdGunIK.rightHandIKTarget = heldModel.transform.Find("RightHandTarget");

                holdGunIK.elbowIKAmount = 1;
                holdGunIK.handIKAmount = 1;
            }
        }
        else
        {
            GatherableSupplies.Value.AbortGather();
        }
    }
}

