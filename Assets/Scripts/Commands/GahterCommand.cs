using UnityEngine;

[CreateAssetMenu(fileName = "Gahter Action", menuName = "Units/Commands/Gahter", order = 105)]
public class GahterCommand : BaseCommand
{
    [SerializeField] private AbstractUnitSO commandPostSO;

    public override bool CanHandle(CommandContext context)
    {
        return context.Commandable is Worker 
            && context.Hit.collider != null 
            && IsGatherableSupplyOrCommandPost(context.Hit.collider);
    }

    public override void Handle(CommandContext context)
    {
        Worker worker = context.Commandable as Worker;
        if(!IsHitColliderVisible(context))
        {
            worker.MoveTo(context.Hit.collider.gameObject.transform.position);
        }
        else if(context.Hit.collider.TryGetComponent(out GatherableSupply supply))
        {
            worker.Gather(supply);
        }
        else if(IsCommandPost(context.Hit.collider) && worker.HasSupplies)
        {
            worker.ReturnSupplies(context.Hit.collider.gameObject);
        }
        else
        {
            worker.MoveTo(context.Hit.collider.gameObject.transform.position);
        }
        
    }

    private bool IsGatherableSupplyOrCommandPost(Collider collider) => collider.TryGetComponent(out GatherableSupply _) || IsCommandPost(collider);

    private bool IsCommandPost(Collider collider) => 
        collider.TryGetComponent(out BaseBuilding building) 
        && building.UnitSO.Equals(commandPostSO);

    public override bool IsLocked(CommandContext commandContext) => false;
}
