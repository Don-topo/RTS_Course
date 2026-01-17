using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Rigidbody cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private new Camera camera;
    [SerializeField] private CameraConfig cameraConfig;
    [SerializeField] private LayerMask selectableUnitsLayers;
    [SerializeField] private LayerMask floorLayers;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private RectTransform selectionBox;    
    [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color errorTintColor = Color.red;
    [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color errorFresnelColor = new(4, 1.7f, 0, 2);
    [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color availableToPlaceTintColor = new(0.2f, 0.65f, 1, 2);
    [SerializeField][ColorUsage(showAlpha: true, hdr: true)] private Color availableToPlaceFresnelColor = new(4, 1.7f, 0, 2);
    [SerializeField] private Renderer clickIndicator;

    private static readonly int TINT = Shader.PropertyToID("_Tint");
    private static readonly int FRESNEL = Shader.PropertyToID("_FresnelColor");
    private static readonly int CLICK_TIME = Shader.PropertyToID("_ClickTime");

    private Vector2 startingMousePosition;

    private BaseCommand activeCommand;
    private GameObject ghostInstance;
    private MeshRenderer ghostRenderer;
    private bool wasMouseDownOnUI;
    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotationStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;
    private HashSet<AbstractUnit> aliveUnits = new(100);
    private HashSet<AbstractUnit> addedUnits = new(24);
    private List<ISelectable> selectedUnits = new(12);

    private void Awake()
    {
        if(!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("Cinemachine Camera did not have CinemachineFollow. Zoom functionality will not work!");
        }

        startingFollowOffset = cinemachineFollow.FollowOffset;
        maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

        Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] += HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] += HandleUnitDeselected;
        Bus<UnitSpawnEvent>.OnEvent[Owner.Player1] += HandleUnitSpawn;
        Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] += HandleActionSelected;
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] += HandleUnitDeath;
        Bus<MinimapClickEvent>.OnEvent[Owner.Player1] += HandleMinimapClick;
    }

    private void Update()
    {
        HandlePanning();
        HandleZooming();
        HandleRotation();
        HandleGhost();
        HandleRightClick();
        HandleDragSelect();
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] -= HandleUnitSelected;
        Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] -= HandleUnitDeselected;
        Bus<UnitSpawnEvent>.OnEvent[Owner.Player1] -= HandleUnitSpawn;
        Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] -= HandleActionSelected;
        Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
        Bus<MinimapClickEvent>.OnEvent[Owner.Player1] -= HandleMinimapClick;
    }

    private void HandleMinimapClick(MinimapClickEvent evt)
    {
        if(evt.Button == MouseButton.Right)
        {
            IssueRightClickCommand(evt.Hit);
        }
        else if(evt.Button == MouseButton.Left)
        {
            ActivateAction(evt.Hit);
        }
    }

    private void HandleUnitSpawn(UnitSpawnEvent evt) => aliveUnits.Add(evt.Unit);

    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        if (!selectedUnits.Contains(evt.Unit))
        {
            selectedUnits.Add(evt.Unit);
        }
    }

    private void HandleUnitDeselected(UnitDeselectedEvent evt) => selectedUnits.Remove(evt.Unit);

    private void HandleActionSelected(CommandSelectedEvent evt)
    {
        activeCommand = evt.Command;
        if(!activeCommand.RequiresClickToActivate)
        {
            ActivateAction(new RaycastHit());
        }
        else if(activeCommand.GhostPrefab != null)
        {
            ghostInstance = Instantiate(activeCommand.GhostPrefab);
            ghostRenderer = ghostInstance.GetComponentInChildren<MeshRenderer>();
        }
    }

    private void HandleDragSelect()
    {
        if (selectionBox == null) return;

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseDown();   
        }
        else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseDrag();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        selectionBox.sizeDelta = Vector2.zero;
        selectionBox.gameObject.SetActive(true);
        startingMousePosition = Mouse.current.position.ReadValue();
        addedUnits.Clear();
        wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void HandleMouseDrag()
    {
        if (activeCommand != null || wasMouseDownOnUI) return;

        Bounds selectionBoxBounds = ResizeSelectionBox();
        foreach (AbstractUnit unit in aliveUnits)
        {
            if(!unit.gameObject.activeInHierarchy) continue;

            Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);
            if (selectionBoxBounds.Contains(unitPosition))
            {
                addedUnits.Add(unit);
            }
        }
    }
    private void HandleMouseUp()
    {
        if(!wasMouseDownOnUI && activeCommand == null && !Keyboard.current.leftShiftKey.isPressed)
        {
            DeselectAllUnits();
        }
        
        HandleLeftClick();
        foreach (AbstractUnit unit in addedUnits)
        {
            unit.Select();
        }
        selectionBox.gameObject.SetActive(false);
    }

    private void DeselectAllUnits()
    {
        ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
        foreach(ISelectable selectable in currentlySelectedUnits)
        {
            selectable.Deselect();
        }
    }

    private Bounds ResizeSelectionBox()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float width = mousePosition.x - startingMousePosition.x;
        float heigth = mousePosition.y - startingMousePosition.y;

        selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, heigth / 2);
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(heigth));

        return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
    }

    private void HandleRightClick()
    {
        if (selectedUnits.Count == 0 || EventSystem.current.IsPointerOverGameObject()) return;

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Mouse.current.rightButton.wasReleasedThisFrame
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers | interactableLayers))
        {
            IssueRightClickCommand(hit);            
        }
    }

    private void IssueRightClickCommand(RaycastHit hit)
    {
        List<AbstractUnit> abstractUnits = new List<AbstractUnit>(selectedUnits.Count);
        foreach (ISelectable selectable in selectedUnits)
        {
            if (selectable is AbstractUnit unit)
            {
                abstractUnits.Add(unit);
            }
        }

        for (int i = 0; i < abstractUnits.Count; i++)
        {
            CommandContext context = new(abstractUnits[i], hit, i, MouseButton.Right);

            foreach (ICommand command in GetAvailableCommands(abstractUnits[i]))
            {
                if (command.CanHandle(context))
                {
                    command.Handle(context);
                    if (command.IsSingleUnitCommand)
                    {
                        return;
                    }
                    break;
                }
            }
        }
        ShowClick(hit.point);
    }

    private void ShowClick(Vector3 position)
    {
        clickIndicator.transform.position = position;
        clickIndicator.material.SetFloat(CLICK_TIME, Time.time);
    }

    private List<BaseCommand> GetAvailableCommands(AbstractUnit unit)
    {
        OverrideCommandsCommand[] overrideCommandsCommands = unit.AvailableCommands
            .Where(command => command is OverrideCommandsCommand)
            .Cast<OverrideCommandsCommand>()
            .ToArray();

        List<BaseCommand> allAvailableCommands = new();
        foreach(OverrideCommandsCommand overrideCommand in overrideCommandsCommands)
        {
            allAvailableCommands.AddRange(overrideCommand.Commands
                .Where(command => command is not OverrideCommandsCommand)
            );
        }

        allAvailableCommands.AddRange(unit.AvailableCommands
            .Where(command => command is not OverrideCommandsCommand)
        );

        return allAvailableCommands;
    }

    private void HandleLeftClick()
    {
        if (camera == null) return;

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (activeCommand == null 
            && addedUnits.Count == 0
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
            && hit.collider.TryGetComponent(out ISelectable selectable))
        {                
            selectable.Select();
        }
        else if(activeCommand != null
            && !EventSystem.current.IsPointerOverGameObject()
            && Physics.Raycast(cameraRay, out hit, float.MaxValue, interactableLayers | floorLayers))
        {
            ActivateAction(hit);
            ShowClick(hit.point);
        }
    }

    private void ActivateAction(RaycastHit hit)
    {
        if(ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }

        List<AbstractCommandable> abstractCommandables = selectedUnits
                        .Where((unit) => unit is AbstractCommandable)
                        .Cast<AbstractCommandable>()
                        .ToList();

        for (int i = 0; i < abstractCommandables.Count; i++)
        {
            CommandContext context = new(abstractCommandables[i], hit, i);       
            if(activeCommand.CanHandle(context))
            {
                activeCommand.Handle(context);
                if(activeCommand.IsSingleUnitCommand)
                {
                    break;
                }
            }             
        }

        Bus<CommandIssuedEvent>.Raise(Owner.Player1, new CommandIssuedEvent(activeCommand));

        activeCommand = null;
    }

    private void HandleRotation()
    {
        if (ShouldSetRotationStartTime())
        {
            rotationStartTime = Time.time;
        }

        float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);
        Vector3 targetFollowOffset;

        if(Keyboard.current.pageDownKey.isPressed)
        {
            targetFollowOffset = new Vector3(
                maxRotationAmount,
                cinemachineFollow.FollowOffset.y,
                0
            );
        }
        else if (Keyboard.current.pageUpKey.isPressed)
        {
            targetFollowOffset = new Vector3(
                -maxRotationAmount,
                cinemachineFollow.FollowOffset.y,
                0
            );
        }
        else
        {
            targetFollowOffset = new Vector3(
                startingFollowOffset.x,
                cinemachineFollow.FollowOffset.y,
                startingFollowOffset.z
            );
        }

        cinemachineFollow.FollowOffset = Vector3.Slerp(
            cinemachineFollow.FollowOffset,
            targetFollowOffset,
            rotationTime
        );
    }

    private void HandleGhost()
    {
        if (ghostInstance == null) return;

        if(Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
            activeCommand = null;
            return;
        }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
        {
            ghostInstance.transform.position = hit.point;
            bool allRestrictionPass = activeCommand.AllRestrictionsPass(hit.point);
            ghostRenderer.material.SetColor(TINT, allRestrictionPass ? availableToPlaceTintColor : errorTintColor);
            ghostRenderer.material.SetColor(FRESNEL, allRestrictionPass ? availableToPlaceFresnelColor : errorFresnelColor);
        }
    }

    private bool ShouldSetRotationStartTime()
    {
        return Keyboard.current.pageUpKey.wasPressedThisFrame
            || Keyboard.current.pageDownKey.wasPressedThisFrame
            || Keyboard.current.pageUpKey.wasReleasedThisFrame
            || Keyboard.current.pageDownKey.wasReleasedThisFrame;
    }

    private void HandleZooming()
    {
        if (ShouldSetZoomStartTime())
        {
            zoomStartTime = Time.time;
        }

        Vector3 targetVectorOffset; 

        float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);

        if (Keyboard.current.endKey.isPressed)
        {
            targetVectorOffset = new Vector3(
                cinemachineFollow.FollowOffset.x,
                cameraConfig.MinZoomDistance,
                cinemachineFollow.FollowOffset.z
            );            
        }
        else
        {
            targetVectorOffset = new Vector3(
                cinemachineFollow.FollowOffset.x,
                startingFollowOffset.y,
                cinemachineFollow.FollowOffset.z
            );
        }

        cinemachineFollow.FollowOffset = Vector3.Slerp(
                cinemachineFollow.FollowOffset,
                targetVectorOffset,
                zoomTime
            );
    }

    private bool ShouldSetZoomStartTime()
    {
        return Keyboard.current.endKey.wasPressedThisFrame
            || Keyboard.current.endKey.wasReleasedThisFrame;
    }

    private void HandlePanning()
    {
        Vector2 moveAmount = GetKeyboardMoveAmount();
        moveAmount += GetMouseMoveAmount();
        cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);        
    }

    private Vector2 GetMouseMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;

        if (!cameraConfig.EnableEdgePan) return moveAmount;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        if(mousePosition.x <= cameraConfig.EdgePanSize)
        {
            moveAmount.x -= cameraConfig.MouseSpeed;
        }
        else if(mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
        {
            moveAmount.x += cameraConfig.MouseSpeed;
        }
        if(mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
        {
            moveAmount.y += cameraConfig.MouseSpeed;
        }
        else if(mousePosition.y <= cameraConfig.EdgePanSize)
        {
            moveAmount.y -= cameraConfig.MouseSpeed;
        }

        return moveAmount;
    }

    private Vector2 GetKeyboardMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;
        if (Keyboard.current.upArrowKey.isPressed)
        {
            moveAmount.y += cameraConfig.KeyboardPanSpeed;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveAmount.x -= cameraConfig.KeyboardPanSpeed;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            moveAmount.y -= cameraConfig.KeyboardPanSpeed;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveAmount.x += cameraConfig.KeyboardPanSpeed;
        }

        return moveAmount;
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        selectedUnits.Remove(evt.Unit);
        aliveUnits.Remove(evt.Unit);
    }
}
