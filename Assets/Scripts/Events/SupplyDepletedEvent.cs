public struct SupplyDepletedEvent : IEvent
{
    public GatherableSupply Supply { get; private set; }
    public SupplyDepletedEvent(GatherableSupply supply)
    {
        Supply = supply;
    }
}