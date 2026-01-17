using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Building Is In Progress", story: "[BaseBuilding] is being built .", category: "Conditions", id: "c321e4517d030310124422dc8c10bf97")]
public partial class BuildingIsInProgressCondition : Condition
{
    [SerializeReference] public BlackboardVariable<BaseBuilding> BaseBuilding;

    public override bool IsTrue()
    {
        return BaseBuilding.Value != null 
            && BaseBuilding.Value.Progress.State == BuildingProgress.BuildingState.Building;
    }
}
