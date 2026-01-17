using UnityEngine;

[CreateAssetMenu(fileName = "Set Rally Point", menuName = "Buildings/Commands/Set Rally Point", order = 121)]
public class SetRallyPointCommand : BaseCommand
{
    [field: SerializeField] public LayerMask IgnoreObjectLayers { get; private set; }

    public override bool CanHandle(CommandContext commandContext) => commandContext.Commandable is BaseBuilding;

    public override void Handle(CommandContext commandContext)
    {
        BaseBuilding building = commandContext.Commandable as BaseBuilding;
        RallyPoint rallyPoint;

        if (commandContext.Hit.collider.gameObject == building.gameObject)
        {
            rallyPoint = new RallyPoint(false, Vector3.zero, null);
        }
        else if (IsOnValidLayer(commandContext.Hit.collider.gameObject)
            && FogVisibilityManager.Instance.IsVisible(commandContext.Hit.collider.transform.position))
        {
            rallyPoint = new RallyPoint(true, commandContext.Hit.point, commandContext.Hit.collider.gameObject);
        }
        else
        {
            rallyPoint = new RallyPoint(true, commandContext.Hit.point, null);
        }

        building.RallyPoint = rallyPoint;
    }

    public override bool IsLocked(CommandContext commandContext) => false;

    private bool IsOnValidLayer(GameObject gameObject)
    {
        return (IgnoreObjectLayers.value & (1 << gameObject.layer)) == 0;
    }
}
