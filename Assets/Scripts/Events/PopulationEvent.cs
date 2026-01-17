using UnityEngine;

public struct PopulationEvent : IEvent
{
    public Owner Owner { get; private set; }
    public int PopulationChange { get; private set; }
    public int PopulationLimitChange { get; private set; }

    public PopulationEvent(Owner owner, int populationChange, int populationLimitChange)
    {
        Owner = owner;
        PopulationChange = populationChange;
        PopulationLimitChange = populationLimitChange;
    }
}
