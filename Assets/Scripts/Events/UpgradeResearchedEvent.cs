using UnityEngine;

public struct UpgradeResearchedEvent : IEvent
{
    public Owner Owner { get; private set; }
    public UpgradeSO Upgrade {  get; private set; }
    public UpgradeResearchedEvent(Owner owner, UpgradeSO upgrade )
    {
        Owner = owner; 
        Upgrade = upgrade;
    }
}
