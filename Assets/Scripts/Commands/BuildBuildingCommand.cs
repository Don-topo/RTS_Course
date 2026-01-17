using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "Build Building", menuName = "Units/Commands/Build Building")]
public class BuildBuildingCommand : BaseCommand, IUnlockableCommand
{
    [field: SerializeField] public BuildingSO Building { get; private set; }

    public override bool CanHandle(CommandContext context)
    {
        if(context.Commandable is not  IBuildingBuilder builder || builder.IsBuilding) return false;

        if(context.Hit.collider != null && context.MouseButton == MouseButton.Right)
        {
            return context.Hit.collider.TryGetComponent(out BaseBuilding building)
                && Building == building.BuildingSO
                && (building.Progress.State == BuildingProgress.BuildingState.Paused 
                    || building.Progress.State == BuildingProgress.BuildingState.Destroyed
            );
        }
        return HasEnoughtSupplies(context) && AllRestrictionsPass(context.Hit.point);
    }

    public UnlockableSO[] GetUnmetDependencies(Owner owner)
    {
        return Building.TechTree.GetUnmetDependencies(owner, Building);
    }

    public override void Handle(CommandContext context)
    {
        IBuildingBuilder builder = (IBuildingBuilder)context.Commandable;
        if(context.Hit.collider != null && context.Hit.collider.TryGetComponent(out BaseBuilding building))
        {
            builder.ResumeBuilding(building);
        }
        else if(AllRestrictionsPass(context.Hit.point) && HasEnoughtSupplies(context))
        {
            builder.Build(Building, context.Hit.point);
        }        
    }

    public override bool IsLocked(CommandContext commandContext) => 
        !HasEnoughtSupplies(commandContext) || !Building.TechTree.IsUnlocked(commandContext.Owner, Building);

    private bool HasEnoughtSupplies(CommandContext context)
    {
        return Building.Cost.Minerals <= Supplies.Minerals[context.Owner] && Building.Cost.Gas <= Supplies.Gas[context.Owner];
    }
}
