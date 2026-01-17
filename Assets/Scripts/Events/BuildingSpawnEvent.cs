using UnityEngine;

public struct BuildingSpawnEvent : IEvent
{
    public BaseBuilding Building { get; private set; }
    public Owner Owner { get; private set; }
    public BuildingSpawnEvent(BaseBuilding building, Owner owner)
    {
        Building = building;
        Owner = owner;
    }
}
