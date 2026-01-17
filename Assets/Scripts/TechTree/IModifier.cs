using UnityEngine;

public interface IModifier
{
    public string PropertyPath { get; }
    public void Apply(AbstractUnitSO unit);
}
