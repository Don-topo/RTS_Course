using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Research Upgrade", menuName = "Tech Tree/Research Upgrade Command", order = 140)]
public class ResearchUpgradeCommand : BaseCommand
{
    [field: SerializeField] public UpgradeSO Upgrade {  get; private set; }

    private Dictionary<Owner, BaseBuilding.QueueUpdatedEvent> updateQueue = new();

    public override bool CanHandle(CommandContext commandContext)
    {
        return commandContext.Commandable is BaseBuilding;
    }

    public override void Handle(CommandContext commandContext)
    {
        BaseBuilding building = commandContext.Commandable as BaseBuilding;

        if (HasEnoughtSupplies(commandContext))
        {
            building.BuildUnlockable(Upgrade);

            if(updateQueue.TryAdd(commandContext.Owner, GetQueueUpdatedFunction(commandContext.Owner, building)))
            {
                building.OnQueueUpdated += updateQueue[commandContext.Owner];
            }
        }
    }

    private BaseBuilding.QueueUpdatedEvent GetQueueUpdatedFunction(Owner owner, BaseBuilding building)
    {
        return (unlockables) => HandleQueueUpdated(owner, building, unlockables);
    }

    private void HandleQueueUpdated(Owner owner, BaseBuilding building, UnlockableSO[] unitsInQueue)
    {
        Debug.Log($"Handle Queue Updated in {Name}");
        if(!unitsInQueue.Contains(Upgrade))
        {
            building.OnQueueUpdated -= updateQueue[owner];
            updateQueue.Remove(owner);
        }
    }

    public override bool IsLocked(CommandContext commandContext)
    {
        bool isLocked = !HasEnoughtSupplies(commandContext) || !Upgrade.TechTree.IsUnlocked(commandContext.Owner, Upgrade);

        if(!isLocked && Upgrade.IsOneTimeUnlock && commandContext.Commandable != null 
            && commandContext.Commandable is BaseBuilding)
        {
            isLocked = updateQueue.ContainsKey(commandContext.Owner);
        }

        return isLocked;
    }
        
    public override bool IsAvailable(CommandContext context)
    {
        if(Upgrade.IsOneTimeUnlock && Upgrade.TechTree.IsResearched(context.Owner, Upgrade))
        {
            return false;
        }

        return Upgrade.TechTree.IsUnlocked(context.Owner, Upgrade);
    }

    private bool HasEnoughtSupplies(CommandContext context)
    {
        return Upgrade.Cost.Minerals <= Supplies.Minerals[context.Owner] 
            && Upgrade.Cost.Gas <= Supplies.Gas[context.Owner];
    }
}
