public struct PlaceholderSpawnEvent : IEvent
{
    public Placeholder Placeholder { get; private set; }
    public PlaceholderSpawnEvent(Placeholder placeholder)
    {
        Placeholder = placeholder;
    }
}