using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIActionButton : MonoBehaviour, IUIElement<BaseCommand, IEnumerable<AbstractCommandable>, UnityAction>, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Tooltip tooltip;

    private bool isActive;
    private Button button;
    private RectTransform rectTransform;
    private Key hotkey;
    private bool wasAssignedThisFrame;

    private static readonly string MINERALS_FORMAT = "{0} <color=#00ACFF>Minerals</color>. ";
    private static readonly string GAS_FORMAT = "{0} <color=#3BEA60>Gas</color>. ";
    private static readonly string DEPENDENCY_FORMAT_NO_COMMA = "<color=#AC0000>{0}</color>.";
    private static readonly string DEPENDENCY_FORMAT_COMMA = "<color=#AC0000>{0}</color>, ";
    private static readonly string POPULATION_FORMAT = "{0} <color=#eeeeee>Population</color> ";
    private static readonly string HOTKEY_FORMAT = "(<color=#FFFF00>{0}</color>)\n";

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        Disable();
    }
    private void Update()
    {
        if(button.interactable 
            && !wasAssignedThisFrame
            && hotkey != Key.None && Keyboard.current[hotkey].wasReleasedThisFrame)
        {
            button.onClick?.Invoke();
        }

        wasAssignedThisFrame = false;
    }

    public void EnableFor(BaseCommand command, IEnumerable<AbstractCommandable> selectedUnits, UnityAction onClick)
    {
        button.onClick.RemoveAllListeners();
        SetIcon(command.Icon);
        hotkey = command.HotKey;
        wasAssignedThisFrame = true;
        button.interactable = selectedUnits
            .Any(commandable => !command.IsLocked(new CommandContext(commandable, new RaycastHit())));
        button.onClick.AddListener(onClick);
        isActive = true;

        if(tooltip != null)
        {
            tooltip.SetText(GetTooltipText(command));
        }
    }

    public void Disable()
    {
        SetIcon(null);
        button.interactable = false;
        button.onClick.RemoveAllListeners();
        isActive = false;
        if(tooltip != null)
        {
            tooltip.Hide();
        }
        CancelInvoke();
    }

    public void SetIcon(Sprite icon)
    {
        if (icon == null)
        {
            this.icon.enabled = false;
        }
        else
        {
            this.icon.sprite = icon;
            this.icon.enabled = true;
        }        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isActive)
        {
            Invoke(nameof(ShowTooltip), 0.5f);
        }        
    }

    private void ShowTooltip()
    {
        if(tooltip != null)
        {
            tooltip.Show();
            tooltip.RectTransform.position = new Vector2(
                rectTransform.position.x + rectTransform.rect.width / 2f,
                rectTransform.position.y + rectTransform.rect.height / 2f
            );
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(tooltip != null)
        {
            tooltip.Hide();
        }
        CancelInvoke();
    }

    private string GetTooltipText(BaseCommand command)
    {
        string tooltipText = command.Name;

        if(command.HotKey != Key.None)
        {
            tooltipText += string.Format(HOTKEY_FORMAT, command.HotKey);
        }
        else
        {
            tooltipText += "\n";
        }

        SupplyCostSO supplyCost = null;
        PopulationConfigSO populationConfig = null;

        if(command is BuildUnitCommand unitCommand)
        {
            supplyCost = unitCommand.Unit.Cost;
            populationConfig = unitCommand.Unit.PopulationConfig;
        }
        else if(command is BuildBuildingCommand buildingCommand)
        {
            supplyCost = buildingCommand.Building.Cost;
        }

        if(supplyCost != null)
        {
            if(supplyCost.Minerals > 0)
            {
                tooltipText += string.Format(MINERALS_FORMAT, supplyCost.Minerals);
            }
            if (supplyCost.Gas > 0)
            {
                tooltipText += string.Format(GAS_FORMAT, supplyCost.Gas);
            }
        }

        if(populationConfig != null && populationConfig.PopulationCost > 0)
        {
            tooltipText += string.Format(POPULATION_FORMAT, populationConfig.PopulationCost);
        }

        if(command.IsLocked(new CommandContext(Owner.Player1, null, new RaycastHit()))
            && command is IUnlockableCommand unlockableCommand)
        {
            UnlockableSO[] dependencies = unlockableCommand.GetUnmetDependencies(Owner.Player1);

            if(dependencies.Length > 0)
            {
                tooltipText += "\nRequires: ";
            }

            for(int i = 0;  i < dependencies.Length; i++)
            {
                tooltipText += i == dependencies.Length - 1
                    ? string.Format(DEPENDENCY_FORMAT_NO_COMMA, dependencies[i].name)
                    : string.Format(DEPENDENCY_FORMAT_COMMA, dependencies[i].name);
            }
        }

        return tooltipText;
    }
}
