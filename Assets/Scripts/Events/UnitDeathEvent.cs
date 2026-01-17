public struct UnitDeathEvent : IEvent
{
    public AbstractUnit Unit {  get; private set; }
    public UnitDeathEvent(AbstractUnit unit)
    {
        Unit = unit;
    }
}
