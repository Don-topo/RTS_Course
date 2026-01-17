using UnityEngine;

public struct BuildingDeathEvent : IEvent
{
    public BaseBuilding Building { get; private set; }
    public Owner Owner { get; private set; }
    public BuildingDeathEvent(BaseBuilding building, Owner owner)
    {
        Building = building;
        Owner = owner;
    }
}
