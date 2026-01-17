public struct PlaceholderDestroyEvent : IEvent
{
    public Placeholder Placeholder { get; private set; }
    public PlaceholderDestroyEvent(Placeholder placeholder)
    {
        Placeholder = placeholder;
    }
}