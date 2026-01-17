using UnityEngine;

public class BaseMilitaryUnit : AbstractUnit, ITransportable
{
    public int TransportCapacityUsage => unitSO.TransportConfig.GetTransportCapacityUsage();

    protected override void Start()
    {
        base.Start();

        graphAgent.SetVariableValue("Command", UnitCommands.Attack);
        graphAgent.SetVariableValue("TargetLocation", transform.position);
    }

    public void LoadInto(ITransporter transporter)
    {
        MoveTo(transporter.Transform);
        transporter.Load(this);
    }
}
