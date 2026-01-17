using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class BaseBuilding : AbstractCommandable
{
    public int QueueSize => buildingQueue.Count;
    public UnlockableSO[] Queue => buildingQueue.ToArray();
    [field: SerializeField] public float CurrentQueueStartTime {  get; private set; }
    [field: SerializeField] public UnlockableSO SOBeingBuild { get; private set; }
    [field: SerializeField] public MeshRenderer MainRenderer { get; private set; }
    [field: SerializeField] public BuildingSO BuildingSO {get; private set;}
    [SerializeField] private Material primaryMaterial;
    [SerializeField] private NavMeshObstacle navMeshObstacle;
    [field: SerializeField] public BuildingProgress Progress { get; private set; } = new(
        BuildingProgress.BuildingState.Destroyed, 0, 0);
    [SerializeField] private new Collider collider;
    [SerializeField] private CancelBuildingCommand cancelBuildingCommand;
    [SerializeField] private LineRenderer rallyPointRenderer;
    private RallyPoint rallyPoint;
    public RallyPoint RallyPoint
    {
        get => rallyPoint;
        set
        {
            if(rallyPointRenderer != null)
            {
                rallyPointRenderer.enabled = value.IsSet;
                rallyPointRenderer.positionCount = 2;
                rallyPointRenderer.SetPositions(new[]
                {
                    transform.position,
                    value.Point
                });
            }

            rallyPoint = value;
        }
    }

    public delegate void QueueUpdatedEvent(UnlockableSO[] unitsInQueue);
    public event QueueUpdatedEvent OnQueueUpdated;

    private List<UnlockableSO> buildingQueue = new(MAX_QUEUE_SIZE);
    private const int MAX_QUEUE_SIZE = 5;
    private Placeholder culledVisuals;
    
    private IBuildingBuilder unitBuildingThis;
    private bool unitHasSubstractedPopulationCost = false;

    protected override void Awake()
    {
        base.Awake();
        BuildingSO = UnitSO as BuildingSO;
        MaxHealth = BuildingSO.Health;
    }

    protected override void Start()
    {
        base.Start();
        if(MainRenderer != null)
        {
            MainRenderer.material = primaryMaterial;
        }
        Progress = new BuildingProgress(BuildingProgress.BuildingState.Completed, Progress.StartTime, 1);
        unitBuildingThis = null;
        Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
        Bus<BuildingSpawnEvent>.Raise(Owner, new BuildingSpawnEvent(this, Owner));

        foreach(UpgradeSO upgrade in BuildingSO.Upgrades)
        {
            if (BuildingSO.TechTree.IsResearched(Owner, upgrade))
            {
                upgrade.Apply(BuildingSO);
            }
        }

        if (UnitSO.PopulationConfig != null)
        {
            Bus<PopulationEvent>.Raise(Owner, new PopulationEvent(
                Owner,
                UnitSO.PopulationConfig.PopulationCost,
                UnitSO.PopulationConfig.PopulationSupply
            ));
        }

        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
        Bus<BuildingDeathEvent>.Raise(Owner, new BuildingDeathEvent(this, Owner));
    }

    public void BuildUnlockable(UnlockableSO unlockable)
    {
        if(buildingQueue.Count == MAX_QUEUE_SIZE)
        {
            Debug.LogError("BuildUnit called when the queue was already full! This is not supported!");
            return;
        }

        Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, -unlockable.Cost.Minerals, unlockable.Cost.MineralsSO));
        Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, -unlockable.Cost.Gas, unlockable.Cost.GasSO));

        buildingQueue.Add(unlockable);
        if(buildingQueue.Count == 1)
        {
            StartCoroutine(DoBuildUnits());
        }
        else
        {
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
        }        
    }

    public void CancelBuildingUnit(int index)
    {
        if (index < 0 || index >= buildingQueue.Count)
        {
            Debug.LogError("Attempting to cancel building a unit outside the bounds of the queue!");
            return;
        }

        UnlockableSO unlockableSO = buildingQueue[index];
        Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, unlockableSO.Cost.Minerals, unlockableSO.Cost.MineralsSO));
        Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, unlockableSO.Cost.Gas, unlockableSO.Cost.GasSO));

        buildingQueue.RemoveAt(index);

        if (index == 0)
        {
            StopAllCoroutines();
            if (unlockableSO is AbstractUnitSO unitSO && unitHasSubstractedPopulationCost)
            {
                Bus<PopulationEvent>.Raise(Owner, new PopulationEvent(
                    Owner,
                    -unitSO.PopulationConfig.PopulationCost,
                    0
                ));
            }
                       
            if(buildingQueue.Count > 0)
            {
                StartCoroutine(DoBuildUnits());
            }
            else
            {
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
        }
        else
        {            
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
        }

    }

    public void CancelBuilding()
    {
        if(unitBuildingThis != null)
        {
            unitBuildingThis.CancelBuilding();
        }
        else
        {
            Destroy(gameObject);

            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(
                Owner, 
                Mathf.FloorToInt(0.75f * UnitSO.Cost.Minerals),
                UnitSO.Cost.MineralsSO
            ));
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(
                Owner, 
                Mathf.FloorToInt(0.75f * UnitSO.Cost.Gas),
                UnitSO.Cost.GasSO)
            );
        }
    }

    private IEnumerator DoBuildUnits()
    {
        while(buildingQueue.Count > 0)
        {
            SOBeingBuild = buildingQueue[0];
            CurrentQueueStartTime = Time.time;
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            unitHasSubstractedPopulationCost = false;

            bool isUnit = SOBeingBuild is AbstractUnitSO;

            if(isUnit)
            {
                AbstractUnitSO unitSO = SOBeingBuild as AbstractUnitSO;
                while (Supplies.Population[Owner] + unitSO.PopulationConfig.PopulationCost > Supplies.PopulationLimit[Owner])
                {
                    yield return null;
                    CurrentQueueStartTime = Time.time;
                }

                Bus<PopulationEvent>.Raise(Owner, new PopulationEvent(
                    Owner,
                    unitSO.PopulationConfig.PopulationCost,
                    0
                ));
                unitHasSubstractedPopulationCost = true;
            }

            yield return new WaitForSeconds(SOBeingBuild.BuildTime);

            if (isUnit) 
            {
                AbstractUnitSO unitSO = SOBeingBuild as AbstractUnitSO;
                GameObject instance = Instantiate(unitSO.Prefab, transform.position, Quaternion.identity);
                if (instance.TryGetComponent(out AbstractCommandable commandable))
                {
                    commandable.Owner = Owner;
                }

                MoveToRallyPoint(instance);
            }
            else if(SOBeingBuild is UpgradeSO upgrade)
            {
                Bus<UpgradeResearchedEvent>.Raise(Owner, new UpgradeResearchedEvent(Owner, upgrade));
            }
            
            buildingQueue.RemoveAt(0);
        }

        unitHasSubstractedPopulationCost = false;

        OnQueueUpdated?.Invoke(buildingQueue.ToArray());
    }

    public void StartBuilding(IBuildingBuilder buildingBuilder)
    {
        Awake();
        unitBuildingThis = buildingBuilder;
        Owner = buildingBuilder.Owner;
        MainRenderer.material = BuildingSO.PlaceMaterial;
        SetCommandsOverrides(new BaseCommand[] { cancelBuildingCommand });

        Progress = new BuildingProgress(
            BuildingProgress.BuildingState.Building,
            Time.time - BuildingSO.BuildTime * Progress.Progress,
            Progress.Progress
        );

        if(Progress.Progress == 0)
        {
            Heal(1);
        }

        if (collider != null)
        {
            collider.enabled = true;
        }

        Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
        Bus<UnitDeathEvent>.OnEvent[Owner] += HandleUnitDeath;
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        if(evt.Unit.TryGetComponent(out IBuildingBuilder buildingBuilder) && buildingBuilder == unitBuildingThis)
        {
            Progress = new BuildingProgress(
                BuildingProgress.BuildingState.Paused,
                Progress.StartTime,
                (Time.time - Progress.StartTime) / BuildingSO.BuildTime
            );

            Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
        }
    }

    private void MoveToRallyPoint(GameObject instance)
    {
        if (!rallyPoint.IsSet || !instance.TryGetComponent(out IMoveable moveable)) return;

        if(rallyPoint.Target == null)
        {
            moveable.MoveTo(rallyPoint.Point);
        }
        else
        {
            if(instance.TryGetComponent(out Worker worker) && rallyPoint.Target.TryGetComponent(out GatherableSupply supply))
            {
                worker.Gather(supply);
            }
            else
            {
                moveable.MoveTo(rallyPoint.Target.transform);
            }            
        }
    }

    protected override void OnGainVisibility()
    {
        base.OnGainVisibility();

        if(culledVisuals != null)
        {
            culledVisuals.gameObject.SetActive(false);
        }
    }

    protected override void OnLoseVisibility()
    {
        base.OnLoseVisibility();

        if(culledVisuals == null)
        {
            Transform originalRendererTransform = MainRenderer.transform;
            GameObject culledGO = new GameObject($"Culled {BuildingSO.Name} Visuals")
            {
                layer = LayerMask.NameToLayer("Supplies"),
                transform =
                {
                    position = originalRendererTransform.position,
                    rotation = originalRendererTransform.rotation,
                    localScale = originalRendererTransform.localScale,
                }
            };
            culledVisuals = culledGO.AddComponent<Placeholder>();
            culledVisuals.Owner = Owner;
            culledVisuals.ParentObject = gameObject;
            MeshFilter meshFilter = culledGO.AddComponent<MeshFilter>();
            meshFilter.mesh = MainRenderer.GetComponent<MeshFilter>().mesh;
            MeshRenderer renderer = culledGO.AddComponent<MeshRenderer>();
            renderer.materials = MainRenderer.materials;
        }
        else
        {
            culledVisuals.gameObject.SetActive(true);
        }
    }

    public override void Select()
    {
        base.Select();
        if (rallyPointRenderer != null)
        {
            rallyPointRenderer.enabled = rallyPoint.IsSet;
        }
    }

    public override void Deselect()
    {
        base.Deselect();
        if(rallyPointRenderer != null)
        {
            rallyPointRenderer.enabled = false;
        }
        if(Progress.State != BuildingProgress.BuildingState.Completed)
        {
            SetCommandsOverrides(new BaseCommand[] { cancelBuildingCommand });
        }
    }
}
