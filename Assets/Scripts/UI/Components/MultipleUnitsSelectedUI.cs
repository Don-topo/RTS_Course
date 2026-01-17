using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultipleUnitsSelectedUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
{
    [SerializeField] private UIUnitSelectedButton[] selectedUnitButtons;

    private HashSet<AbstractCommandable> selectedUnits;

    public void Disable() => gameObject.SetActive(false);

    public void EnableFor(HashSet<AbstractCommandable> items)
    {
        gameObject.SetActive(true);

        if(selectedUnitButtons.Length < items.Count)
        {
            Debug.LogWarning($"Too many units were passed to MultiUnitsSelectedUI! Ensure no more"
                + $" than {selectedUnitButtons.Length} units are selected at a time, or update the UI to handle more units!");
        }

        int i = 0;
        foreach(AbstractCommandable commandable in items)
        {
            selectedUnitButtons[i].EnableFor(commandable, () => HandleClick(commandable));
            i++;
        }

        for(; i < selectedUnitButtons.Length; i++)
        {
            selectedUnitButtons[i].Disable();
        }
        selectedUnits = items;
    }

    private void HandleClick(AbstractCommandable clickedCommandable)
    {
        if(Keyboard.current.shiftKey.isPressed)
        {
            clickedCommandable.Deselect();
        }
        else
        {
            selectedUnits.Remove(clickedCommandable);

            AbstractCommandable[] commandables = new AbstractCommandable[selectedUnits.Count];
            selectedUnits.CopyTo(commandables, 0);

            foreach(AbstractCommandable commandable in commandables)
            {
                commandable.Deselect();
            }

            clickedCommandable.Select();
        }
    }
}
