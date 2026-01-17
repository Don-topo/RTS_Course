using UnityEngine;

[CreateAssetMenu(fileName = "Load Unit", menuName = "Units/Commands/Load Unit", order = 106)]
public class LoadUnitCommand : BaseCommand
{
    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is ITransporter transporter
            && context.Hit.collider != null
            && context.Hit.collider.TryGetComponent(out ITransportable transportable)
            && transporter.Owner == transportable.Owner;
    }

    public override void Handle(CommandContext context)
    {
        ITransporter transporter = context.Commandable as ITransporter;
        ITransportable transportable = context.Hit.collider.GetComponent<ITransportable>();

        transporter.Load(transportable);
    }

    public override bool IsLocked(CommandContext context)
    {
        if(context.Commandable is ITransporter transporter)
        {
            return transporter.UsedCapacity >= transporter.Capacity;
        }
        return true;
    }
}
