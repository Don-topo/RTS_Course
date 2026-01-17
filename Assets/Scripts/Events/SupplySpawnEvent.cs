public struct SupplySpawnEvent : IEvent
{
    public GatherableSupply Supply { get; private set; }
    public SupplySpawnEvent(GatherableSupply supply)
    {
        Supply = supply;
    }
}