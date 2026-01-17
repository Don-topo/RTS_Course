using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public struct MinimapClickEvent : IEvent
{
    public MouseButton Button { get; }
    public RaycastHit Hit { get; }

    public MinimapClickEvent(MouseButton button, RaycastHit hit)
    {
        Button = button; 
        Hit = hit;
    }
}
