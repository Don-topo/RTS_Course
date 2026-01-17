using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlGroupUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
{
    [SerializeField] private ControlGroupKeyboardHotKey[] controlGroupHotKeys;

    private HashSet<AbstractCommandable> selectedUnits;

    private void Update()
    {
        if (!Keyboard.current.ctrlKey.isPressed) return;

        foreach(ControlGroupKeyboardHotKey groupHotKey in controlGroupHotKeys)
        {
            if (Keyboard.current[groupHotKey.Key].wasReleasedThisFrame && selectedUnits.Count > 0)
            {
                groupHotKey.Group.EnableFor(selectedUnits, groupHotKey.Key, SelectUnits);
            }
        }
    }

    private void SelectUnits(HashSet<AbstractCommandable> units)
    {
        foreach(ISelectable selectable in selectedUnits.ToList())
        {
            selectable.Deselect();
        }

        foreach(ISelectable selectable in units)
        {
            selectable.Select();
        }
    }

    public void Disable() { }

    public void EnableFor(HashSet<AbstractCommandable> items)
    {
        selectedUnits = items;
    }

    [System.Serializable]
    private struct ControlGroupKeyboardHotKey
    {
        [field: SerializeField] public Key Key { get; private set; }
        [field: SerializeField] public ControlGroup Group { get; private set; }
    }
}
