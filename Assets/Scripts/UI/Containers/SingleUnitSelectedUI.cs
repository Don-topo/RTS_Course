using TMPro;
using UnityEngine;

public class SingleUnitSelectedUI : MonoBehaviour, IUIElement<AbstractCommandable>
{
    [SerializeField] private TextMeshProUGUI unitName;
    [SerializeField] private StatIcon damageIcon;

    public void Disable()
    {
        gameObject.SetActive(false);
        damageIcon.Disable();
    }

    public void EnableFor(AbstractCommandable commandable)
    {
        gameObject.SetActive(true);
        unitName.SetText(commandable.UnitSO.Name);
        damageIcon.EnableFor(commandable);
    }
}
