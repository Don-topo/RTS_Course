using UnityEngine;

public class Placeholder : MonoBehaviour, IHideable
{
    public Transform Transform => this == null ? null : transform;
    public bool IsVisible { get; private set; }
    public Owner Owner { get; set; }
    public GameObject ParentObject { get; set; }

    public event IHideable.VisibilityChangeEvent OnVisibilityChanged;

    private void Start()
    {
        Bus<PlaceholderSpawnEvent>.Raise(Owner, new PlaceholderSpawnEvent(this));
    }

    private void OnDestroy()
    {
        Bus<PlaceholderDestroyEvent>.Raise(Owner, new PlaceholderDestroyEvent(this));
    }

    public void SetVisible(bool isVisible)
    {
        if(isVisible != IsVisible)
        {
            OnVisibilityChanged?.Invoke(this, isVisible);
        }

        IsVisible = isVisible;

        if(isVisible && ParentObject == null)
        {
            Destroy(gameObject);
        }
    }
}
