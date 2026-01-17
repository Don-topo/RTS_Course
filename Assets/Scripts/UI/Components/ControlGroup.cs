using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ControlGroup : MonoBehaviour, 
    IUIElement<HashSet<AbstractCommandable>, Key, UnityAction<HashSet<AbstractCommandable>>>
{
    [SerializeField] private Image unitIcon;
    [SerializeField] private TextMeshProUGUI groupText;
    [SerializeField] private TextMeshProUGUI unitCountText;

    private HashSet<AbstractCommandable> unitsInGroup;
    private Button button;
    private Key hotKey;
    private UnityAction<HashSet<AbstractCommandable>> onActivate;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (Keyboard.current[hotKey].wasReleasedThisFrame)
        {
            onActivate?.Invoke(unitsInGroup);
        }
    }

    private void OnDestroy()
    {
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
    }

    public void Disable()
    {
        button.onClick.RemoveAllListeners();
        gameObject.SetActive(false);
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
    }

    public void EnableFor(HashSet<AbstractCommandable> items, Key hotKey, UnityAction<HashSet<AbstractCommandable>> callback)
    {
        unitsInGroup = items.ToHashSet();
        this.hotKey = hotKey;
        onActivate = callback;
        gameObject.SetActive(true);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback(unitsInGroup));

        SetIconAndUnitCountText();
    }

    private void SetIconAndUnitCountText()
    {
        unitCountText.SetText(unitsInGroup.Count.ToString());
        unitIcon.sprite = unitsInGroup.First().UnitSO.Icon;
    }

    private void OnEnable()
    {
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] += HandleUnitDeath;
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        unitsInGroup.Remove(evt.Unit);

        if(unitsInGroup.Count == 0)
        {
            Disable();
            return;
        }

        SetIconAndUnitCountText();
    }
}
