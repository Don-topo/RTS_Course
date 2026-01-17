using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RuntimeUI : MonoBehaviour
{
    [SerializeField] private ActionsUI actionsUI;
    [SerializeField] private UnitIconUI unitIconUI;
    [SerializeField] private SingleUnitSelectedUI singleUnitSelectedUI;
    [SerializeField] private BuildingSelectedUI buildingSelectedUI;
    [SerializeField] private UnitTransportUI unitTransportUI;
    [SerializeField] private MultipleUnitsSelectedUI multipleUnitsSelectedUI;
    [SerializeField] private ControlGroupUI controlGroupUI;

    private HashSet<AbstractCommandable> selectedUnits = new(12);

    private void Awake()
    {
        Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] += HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] += HandleUnitDeselected;
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] += HandleUnitDeath;
        Bus<SupplyEvent>.OnEvent[Owner.Player1] += HandleSupplyChange;
        Bus<UnitLoadEvent>.OnEvent[Owner.Player1] += HandleLoadUnit;
        Bus<UnitUnloadEvent>.OnEvent[Owner.Player1] += HandleUnloadUnit;
        Bus<BuildingSpawnEvent>.OnEvent[Owner.Player1] += HandleBuildingSpawn;
        Bus<UpgradeResearchedEvent>.OnEvent[Owner.Player1] += HandleUpgradeResearched;
        Bus<BuildingDeathEvent>.OnEvent[Owner.Player1] += HandleBuildingDeath;
    }

    private void Start()
    {
        actionsUI.Disable();
        unitIconUI.Disable();
        buildingSelectedUI.Disable();
        singleUnitSelectedUI.Disable();
        unitTransportUI.Disable();
        multipleUnitsSelectedUI.Disable();
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] -= HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] -= HandleUnitDeselected;
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
        Bus<SupplyEvent>.OnEvent[Owner.Player1] -= HandleSupplyChange;
        Bus<UnitLoadEvent>.OnEvent[Owner.Player1] -= HandleLoadUnit;
        Bus<UnitUnloadEvent>.OnEvent[Owner.Player1] -= HandleUnloadUnit;
        Bus<BuildingSpawnEvent>.OnEvent[Owner.Player1] -= HandleBuildingSpawn;
        Bus<UpgradeResearchedEvent>.OnEvent[Owner.Player1] -= HandleUpgradeResearched;
        Bus<BuildingDeathEvent>.OnEvent[Owner.Player1] -= HandleBuildingDeath;
    }

    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        if(evt.Unit is AbstractCommandable commandable)
        {
            selectedUnits.Add(commandable);
            RefreshUI();
        }
    }

    private void HandleUnitDeselected(UnitDeselectedEvent evt)
    {
        if(evt.Unit is AbstractCommandable commandable)
        {
            selectedUnits.Remove(commandable);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        controlGroupUI.EnableFor(selectedUnits);
        if (selectedUnits.Count > 0)
        {
            actionsUI.EnableFor(selectedUnits);

            if(selectedUnits.Count == 1)
            {
                multipleUnitsSelectedUI.Disable();
                ResolveSingleUnitSelectedUI();
            }
            else
            {
                unitIconUI.Disable();
                singleUnitSelectedUI.Disable();
                buildingSelectedUI.Disable();
                unitTransportUI.Disable();
                multipleUnitsSelectedUI.EnableFor(selectedUnits);
            }
        }
        else
        {
            DisableAllContainers();
        }
    }

    private void DisableAllContainers()
    {
        actionsUI.Disable();
        buildingSelectedUI.Disable();
        unitIconUI.Disable();
        singleUnitSelectedUI.Disable();
        unitTransportUI.Disable();
        multipleUnitsSelectedUI.Disable();
    }

    private void ResolveSingleUnitSelectedUI()
    {
        AbstractCommandable commandable = selectedUnits.First();
        unitIconUI.EnableFor(commandable);

        if (commandable is BaseBuilding building)
        {
            singleUnitSelectedUI.Disable();
            unitTransportUI.Disable();
            buildingSelectedUI.EnableFor(building);
        }
        else if (commandable is ITransporter transporter && transporter.UsedCapacity > 0)
        {
            unitTransportUI.EnableFor(transporter);
            buildingSelectedUI.Disable();
            singleUnitSelectedUI.Disable();
        }
        else
        {
            buildingSelectedUI.Disable();
            unitTransportUI.Disable();
            singleUnitSelectedUI.EnableFor(commandable);
        }
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        selectedUnits.Remove(evt.Unit);
        RefreshUI();
    }

    private void HandleSupplyChange(SupplyEvent evt)
    {
        actionsUI.EnableFor(selectedUnits);
    }

    private void HandleLoadUnit(UnitLoadEvent evt)
    {
        if (selectedUnits.Count == 1 && selectedUnits.First() is ITransporter)
        {
            RefreshUI();
        }else if(evt.Unit is AbstractCommandable commandable && selectedUnits.Contains(commandable))
        {
            commandable.Deselect();
        }
    }

    private void HandleUnloadUnit(UnitUnloadEvent evt)
    {
        if(selectedUnits.Count == 1 && selectedUnits.First() is ITransporter)
        {
            RefreshUI();
        }
    }

    private void HandleBuildingSpawn(BuildingSpawnEvent evt)
    {
        actionsUI.EnableFor(selectedUnits);
    }

    private void HandleUpgradeResearched(UpgradeResearchedEvent evt)
    {
        RefreshUI();
    }

    private void HandleBuildingDeath(BuildingDeathEvent evt)
    {
        selectedUnits.Remove(evt.Building);
        RefreshUI();
    }
}
