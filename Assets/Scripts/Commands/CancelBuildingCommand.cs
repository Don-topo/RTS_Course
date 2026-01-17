using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[CreateAssetMenu(fileName = "Cancel Building", menuName = "Units/Commands/Cancel Building")]
public class CancelBuildingCommand : BaseCommand
{
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is BaseBuilding ||
            (context.Commandable is IBuildingBuilder
            && context.MouseButton == MouseButton.Left)
        ;
    }

    public override void Handle(CommandContext context)
    {
        if(context.Commandable is BaseBuilding building)
        {
            building.CancelBuilding();
        }
        else if(context.Commandable is IBuildingBuilder buildingBuilder)
        {
            buildingBuilder.CancelBuilding();
        }                
    }

    public override bool IsLocked(CommandContext commandContext) => false;
}
