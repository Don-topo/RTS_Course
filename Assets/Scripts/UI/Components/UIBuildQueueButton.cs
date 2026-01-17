using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBuildQueueButton : MonoBehaviour, IUIElement<UnlockableSO, UnityAction>
{
    [SerializeField] private Image icon;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        Disable();
    }

    public void Disable()
    {
        button.interactable = false;
        button.onClick.RemoveAllListeners();
        icon.gameObject.SetActive(false);
    }

    public void EnableFor(UnlockableSO item, UnityAction callback)
    {
        button.onClick.RemoveAllListeners();
        button.interactable = true;
        button.onClick.AddListener(callback);
        icon.gameObject.SetActive(true);
        icon.sprite = item.Icon;
    }
}
