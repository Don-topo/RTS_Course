using UnityEngine;

public struct CommandIssuedEvent : IEvent
{
    public BaseCommand Command { get; }

    public CommandIssuedEvent(BaseCommand command)
    {
        Command = command;
    }
}
