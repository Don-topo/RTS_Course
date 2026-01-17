using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ActionsUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
{
    [SerializeField] private UIActionButton[] actionButtons;

    private HashSet<BaseBuilding> selectedBuildings = new();


    private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
    {
        IEnumerable<BaseCommand> availableCommands = selectedUnits.Count > 0 ? 
            selectedUnits.ElementAt(0).AvailableCommands : Array.Empty<BaseCommand>();

        if(availableCommands != null)
        {
            availableCommands = availableCommands.Where(action => action.IsAvailable(
                new CommandContext(
                    Owner.Player1,
                    selectedUnits.FirstOrDefault(),
                    new RaycastHit()
                )
            ));
        }
        else
        {
            availableCommands = Array.Empty<BaseCommand>();
        }
        
        foreach(AbstractCommandable commandable in selectedUnits)
        {
            if(commandable.AvailableCommands != null)
            {
                availableCommands = availableCommands.Intersect(commandable.AvailableCommands);
            }            
        }

        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (availableCommands == null) return;

            BaseCommand actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();

            if (actionForSlot != null)
            {
                actionButtons[i].EnableFor(actionForSlot, selectedUnits, HandleClick(actionForSlot));
            }
            else
            {
                actionButtons[i].Disable();
            }
        }
    }

    private UnityAction HandleClick(BaseCommand action)
    {
        return () => Bus<CommandSelectedEvent>.Raise(Owner.Player1, new CommandSelectedEvent(action));
    }

    public void EnableFor(HashSet<AbstractCommandable> selectedUnits)
    {
        RefreshButtons(selectedUnits);

        foreach(BaseBuilding building in selectedBuildings)
        {
            building.OnQueueUpdated -= OnbuildingQueueUpdated;
        }

        selectedBuildings = selectedUnits
            .Where(selectedUnit => selectedUnit is BaseBuilding)
            .Cast<BaseBuilding>()
            .ToHashSet();

        foreach(BaseBuilding building in selectedBuildings)
        {
            building.OnQueueUpdated += OnbuildingQueueUpdated;
        }
    }

    public void Disable()
    {
        foreach (UIActionButton actionButton in actionButtons)
        {
            actionButton.Disable();
        }

        foreach (BaseBuilding building in selectedBuildings)
        {
            building.OnQueueUpdated -= OnbuildingQueueUpdated;
        }
        selectedBuildings.Clear();
    }

    private void OnbuildingQueueUpdated(UnlockableSO[] unitsInQueue)
    {
        RefreshButtons(selectedBuildings.Cast<AbstractCommandable>().ToHashSet());
    }
}
