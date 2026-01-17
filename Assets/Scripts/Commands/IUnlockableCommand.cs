using UnityEngine;

public interface IUnlockableCommand
{
    public UnlockableSO[] GetUnmetDependencies(Owner owner);
}
