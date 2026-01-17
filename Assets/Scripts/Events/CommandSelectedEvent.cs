using UnityEngine;

public struct CommandSelectedEvent : IEvent
{
    public BaseCommand Command { get; }
    public CommandSelectedEvent(BaseCommand command)
    {
        Command = command;
    }
}
