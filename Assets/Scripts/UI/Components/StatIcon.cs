using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatIcon : MonoBehaviour, IUIElement<AbstractCommandable>, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI upgradeLabel;
    [SerializeField] private TextMeshProUGUI amountLabel;
    [SerializeField] private Tooltip tooltip;
    [SerializeField] private Image icon;

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void EnableFor(AbstractCommandable item)
    {
        if (item.UnitSO == null || item.UnitSO is not UnitSO unitSO || unitSO.AttackConfig == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        int amount = unitSO.AttackConfig.Damage;

        amountLabel.SetText(amount.ToString());
        tooltip.SetText($"{amount} Damage");
        icon.sprite = unitSO.AttackConfig.Icon;

        int upgradeCount = unitSO.Upgrades.Count((upgradeSO) =>
            unitSO.TechTree.IsResearched(item.Owner, upgradeSO) 
                && upgradeSO.PropertyPath.Contains("AttackConfig/Damage")
        );

        upgradeLabel.SetText(upgradeCount.ToString());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Invoke(nameof(ShowTooltip), tooltip.HoverDelay);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelInvoke();
        tooltip.Hide();
    }

    private void ShowTooltip()
    {
        tooltip.Show();
    }
}
