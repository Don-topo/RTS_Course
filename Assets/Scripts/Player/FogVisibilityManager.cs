using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Camera))]
public class FogVisibilityManager : MonoBehaviour
{
    public static FogVisibilityManager Instance { get; private set; }
    private Camera fogOfWarCamera;
    private Texture2D visionTexture;
    private Rect textureRect;
    private HashSet<IHideable> hideables = new(1000);

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError($"Multiple FogVisibilityManagers cannot exist! Disabling {name}");
            enabled = false;
            return;
        }
        Instance = this;

        fogOfWarCamera = GetComponent<Camera>();
        visionTexture = new Texture2D(fogOfWarCamera.targetTexture.width, fogOfWarCamera.targetTexture.height);
        textureRect = new Rect(0, 0, visionTexture.width, visionTexture.height);

        Bus<UnitSpawnEvent>.RegisterForAll(HandleUnitSpawn);
        Bus<UnitDeathEvent>.RegisterForAll(HandleUnitDeath);

        Bus<BuildingSpawnEvent>.RegisterForAll(HandleBuildingSpawn);
        Bus<BuildingDeathEvent>.RegisterForAll(HandleBuildingDeath);

        Bus<PlaceholderSpawnEvent>.RegisterForAll(HandlePlaceholderSpawn);
        Bus<PlaceholderDestroyEvent>.RegisterForAll(HandlePlaceholderDestroy);

        Bus<SupplySpawnEvent>.OnEvent[Owner.Unowned] += HandleSupplySpawn;
        Bus<SupplyDepletedEvent>.OnEvent[Owner.Unowned] += HandleSupplyDepleted;
    }

    private void OnDestroy()
    {
        Bus<UnitSpawnEvent>.UnregisterForAll(HandleUnitSpawn);
        Bus<UnitDeathEvent>.UnregisterForAll(HandleUnitDeath);

        Bus<BuildingSpawnEvent>.UnregisterForAll(HandleBuildingSpawn);
        Bus<BuildingDeathEvent>.UnregisterForAll(HandleBuildingDeath);

        Bus<PlaceholderSpawnEvent>.UnregisterForAll(HandlePlaceholderSpawn);
        Bus<PlaceholderDestroyEvent>.UnregisterForAll(HandlePlaceholderDestroy);

        Bus<SupplySpawnEvent>.OnEvent[Owner.Unowned] -= HandleSupplySpawn;
        Bus<SupplyDepletedEvent>.OnEvent[Owner.Unowned] -= HandleSupplyDepleted;
    }

    private void LateUpdate()
    {
        ReadPixelsToVisionTexture();

        foreach(IHideable hideable in hideables)
        {
            SetUnitVisibilityStatus(hideable);
        }
    }

    public bool IsVisible(Vector3 position)
    {
        Vector3 screenPoint = fogOfWarCamera.WorldToScreenPoint(position);
        Color visibilityColor = visionTexture.GetPixel((int)screenPoint.x, (int)screenPoint.y);
        return visibilityColor.r > 0.9f;
    }

    private void ReadPixelsToVisionTexture()
    {
        RenderTexture previousRenderTexture = RenderTexture.active;

        RenderTexture.active = fogOfWarCamera.targetTexture;
        visionTexture.ReadPixels(textureRect, 0, 0);
        RenderTexture.active = previousRenderTexture;
    }

    private void SetUnitVisibilityStatus(IHideable hideable)
    {
        
        hideable.SetVisible(IsVisible(hideable.Transform.position));
    }

    private void HandleUnitSpawn(UnitSpawnEvent evt)
    {
        if(evt.Unit.Owner != Owner.Player1)
        {
            hideables.Add(evt.Unit);
        }
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        hideables.Remove(evt.Unit);
    }

    private void HandleBuildingSpawn(BuildingSpawnEvent evt)
    {
        if (evt.Building.Owner != Owner.Player1)
        {
            hideables.Add(evt.Building);
        }
    }

    private void HandleBuildingDeath(BuildingDeathEvent evt)
    {
        hideables.Remove(evt.Building);
    }

    private void HandleSupplySpawn(SupplySpawnEvent evt)
    {
        hideables.Add(evt.Supply);
    }

    private void HandleSupplyDepleted(SupplyDepletedEvent evt)
    {
        hideables.Remove(evt.Supply);
    }

    private void HandlePlaceholderSpawn(PlaceholderSpawnEvent evt)
    {
        hideables.Add(evt.Placeholder);
    }

    private void HandlePlaceholderDestroy(PlaceholderDestroyEvent evt)
    {
        hideables.Remove(evt.Placeholder);
    }
}
