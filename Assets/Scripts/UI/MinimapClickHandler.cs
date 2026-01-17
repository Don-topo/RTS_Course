using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent (typeof(EventTrigger))]
public class MinimapClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, 
                                                    IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private LayerMask floorMask;

    private bool isMouseDownOnMinimap;
    private RectTransform rectTransform;
    private BaseCommand activeCommand;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if(minimapCamera == null ||cameraTarget  == null)
        {
            Debug.LogError("MinimapClickHandler is missing some references! Ensure minimapCamera and cameraTarget are assigned!");
            enabled = false;
            return;
        }

        Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] += HandleCommandSelected;
        Bus<CommandIssuedEvent>.OnEvent[Owner.Player1] += HandleCommandIssued;
    }

    private void OnDestroy()
    {
        Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] -= HandleCommandSelected;
        Bus<CommandIssuedEvent>.OnEvent[Owner.Player1] -= HandleCommandIssued;
    }

    private void HandleCommandSelected(CommandSelectedEvent evt) => activeCommand = evt.Command;

    private void HandleCommandIssued(CommandIssuedEvent evt) => activeCommand = null;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left && activeCommand == null)
        {
            isMouseDownOnMinimap = true;
            MoveVirtualCameraTarget(eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData) => isMouseDownOnMinimap = false;

    public void OnPointerMove(PointerEventData eventData) => MoveVirtualCameraTarget(eventData.position);

    public void OnPointerUp(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            isMouseDownOnMinimap = false;
            RaiseClickEvent(eventData.position, MouseButton.Left);
        }
        else if(eventData.button== PointerEventData.InputButton.Right)
        {
            RaiseClickEvent(eventData.position, MouseButton.Right);
        }
    }

    private void MoveVirtualCameraTarget(Vector3 mousePosition)
    {
        if (!isMouseDownOnMinimap) return;

        if(RaycastFromMousePosition(mousePosition, out RaycastHit hit))
        {
            cameraTarget.position = hit.point;
        }
    }

    private void RaiseClickEvent(Vector2 mousePosition, MouseButton button)
    {
        if(RaycastFromMousePosition(mousePosition, out RaycastHit hit))
        {
            Bus<MinimapClickEvent>.Raise(Owner.Player1, new MinimapClickEvent(button, hit));
        }
    }

    private bool RaycastFromMousePosition(Vector2 mousePosition, out RaycastHit hit)
    {
        float widthMultiplier = minimapCamera.scaledPixelWidth / rectTransform.rect.width;
        float heightMultiplier = minimapCamera.scaledPixelHeight / rectTransform.rect.height;

        Vector2 convertedMousePosition = new(
            mousePosition.x * widthMultiplier,
            mousePosition.y * heightMultiplier
        );

        Ray cameraRay = minimapCamera.ScreenPointToRay(convertedMousePosition);
        return Physics.Raycast(cameraRay, out hit, float.MaxValue, floorMask);
        
    }
}
