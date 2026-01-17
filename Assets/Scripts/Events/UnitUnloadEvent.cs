using UnityEngine;

public struct UnitUnloadEvent : IEvent
{
    public ITransportable Unit {  get; private set; }
    public ITransporter Transporter { get; private set; }
    public UnitUnloadEvent(ITransportable unit, ITransporter transporter)
    {
        Unit = unit;
        Transporter = transporter;
    }
}
