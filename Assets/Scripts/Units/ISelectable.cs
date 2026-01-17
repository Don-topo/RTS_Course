
public interface ISelectable
{
    public bool IsSelected { get; }
    void Select();
    void Deselect();
}