using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public struct CommandContext
{
    public AbstractCommandable Commandable { get; private set; }
    public RaycastHit Hit { get; private set; }
    public int UnitIndex { get; private set; }
    public MouseButton MouseButton {  get; private set; }
    public Owner Owner { get; private set; }

    public CommandContext(
        AbstractCommandable commandable, 
        RaycastHit hit, 
        int unitIndex = 0, 
        MouseButton mouseButton = MouseButton.Left)
    {
        Commandable = commandable;
        Hit = hit;
        UnitIndex = unitIndex;
        MouseButton = mouseButton;
        Owner = Owner.Player1;
    }

    public CommandContext(
        Owner owner,
        AbstractCommandable commandable,
        RaycastHit hit,
        int unitIndex = 0,
        MouseButton mouseButton = MouseButton.Left)
    {
        Commandable = commandable;
        Hit = hit;
        UnitIndex = unitIndex;
        MouseButton = mouseButton;
        Owner = owner;
    }
}
