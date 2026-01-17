using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BaseCommand : ScriptableObject, ICommand
{
    [field: SerializeField] public string Name { get; private set; } = "Command";
    [field: SerializeField] public Key HotKey { get; private set; } = Key.None;
    [field: SerializeField] public Sprite Icon {  get; private set; }
    [field: Range(-1, 8)][field: SerializeField] public int Slot { get; private set; }
    [field: SerializeField] public bool IsSingleUnitCommand { get; private set; }
    [field: SerializeField] public bool RequiresClickToActivate {  get; private set; }
    [field: SerializeField] public GameObject GhostPrefab {  get; private set; }
    [field: SerializeField] public BuildingRestrictionSO[] Restrictions { get; private set; }
    public abstract bool CanHandle(CommandContext commandContext);
    public abstract void Handle(CommandContext commandContext);
    public abstract bool IsLocked(CommandContext commandContext);

    public virtual bool IsAvailable(CommandContext context) => true;

    public bool AllRestrictionsPass(Vector3 point) =>
        Restrictions.Length == 0 || Restrictions.All(restriction => restriction.CanPlace(point));

    public bool IsHitColliderVisible(CommandContext context) => context.Hit.collider != null
        && context.Hit.collider.TryGetComponent(out IHideable hideable) && hideable.IsVisible;
}
