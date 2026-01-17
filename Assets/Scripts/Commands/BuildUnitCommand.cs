using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Built Unit", menuName = "Buildings/Commands/Build Unit", order = 120)]
public class BuildUnitCommand : BaseCommand, IUnlockableCommand
{
    [field: SerializeField] public AbstractUnitSO Unit { get; private set; }

    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is BaseBuilding && HasEnoughtSupplies(context);
    }

    public UnlockableSO[] GetUnmetDependencies(Owner owner)
    {
        return Unit.TechTree.GetUnmetDependencies(owner, Unit);
    }

    public override void Handle(CommandContext context)
    {
        BaseBuilding building = (BaseBuilding)context.Commandable;

        if (!HasEnoughtSupplies(context) || (building.QueueSize == 0 && !HasEnoughtPopulation(context))) return;

        building.BuildUnlockable(Unit);
    }

    public override bool IsLocked(CommandContext commandContext) => 
        !HasEnoughtSupplies(commandContext) 
        || !Unit.TechTree.IsUnlocked(commandContext.Owner, Unit)
        || (
            commandContext.Commandable is BaseBuilding building 
                && building.QueueSize == 0 
                && !HasEnoughtPopulation(commandContext)
        );

    private bool HasEnoughtSupplies(CommandContext context)
    {
        return Unit.Cost.Minerals <= Supplies.Minerals[context.Owner] && Unit.Cost.Gas <= Supplies.Gas[context.Owner];
    }

    private bool HasEnoughtPopulation(CommandContext commandContext)
    {
        if(Unit.PopulationConfig == null) return true;

        int newPopulation = Unit.PopulationConfig.PopulationCost + Supplies.Population[commandContext.Owner];

        return newPopulation <= Supplies.PopulationLimit[commandContext.Owner];
    }
}
