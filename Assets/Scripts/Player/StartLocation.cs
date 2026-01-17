using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StartLocation : MonoBehaviour
{
    [SerializeField] private Owner player;
    [SerializeField] private BuildingSO startingBuilding;
    [SerializeField] private UnitSpawnData[] startingUnits;
    [SerializeField] private StartingSupplyData[] startingSupplies;

    private IEnumerator Start()
    {
        GameObject buildingGO = Instantiate(startingBuilding.Prefab, transform.position, transform.rotation);
        BaseBuilding buildingInstance = buildingGO.GetComponent<BaseBuilding>();
        buildingInstance.Owner = player;
        buildingInstance.enabled = true;
        buildingInstance.Heal(startingBuilding.Health);

        yield return null;
        Bounds bounds = new(transform.position, Vector3.one);
        if(buildingInstance.TryGetComponent(out Collider collider))
        {
            bounds = collider.bounds;
        }

        int totalUnits = startingUnits.Sum(item => item.NumberToSpawn);

        for(int i = 0; i < startingUnits.Length; i++)
        {
            for(int count = 0; count < startingUnits[i].NumberToSpawn; count++)
            {
                float offset = (i + count) / (float)totalUnits * bounds.size.x;

                Vector3 spawnLocation = new(bounds.min.x + offset,
                    bounds.min.y,
                    bounds.min.z
                );

                GameObject unitGO = Instantiate(
                    startingUnits[i].UnitSO.Prefab, 
                    spawnLocation, 
                    Quaternion.Euler(0, Random.value * 180, 0)
                );
                AbstractCommandable commandable = unitGO.GetComponent<AbstractCommandable>();
                commandable.Owner = player;

                Bus<PopulationEvent>.Raise(player, new PopulationEvent(
                    player,
                    commandable.UnitSO.PopulationConfig.PopulationCost,
                    commandable.UnitSO.PopulationConfig.PopulationSupply
                ));
            }
        }

        foreach(StartingSupplyData supplyData in startingSupplies)
        {
            Bus<SupplyEvent>.Raise(player, new SupplyEvent(player, supplyData.StartingAmount, supplyData.SupplySO));
        }

        Destroy(GetComponentInChildren<DecalProjector>());
        enabled = false;
    }

    [System.Serializable]
    private struct UnitSpawnData
    {
        [field: SerializeField] public AbstractUnitSO UnitSO { get; private set; }
        [field: SerializeField] public int NumberToSpawn { get; private set; }
    }

    [System.Serializable]
    private struct StartingSupplyData
    {
        [field: SerializeField] public SupplySO SupplySO { get; private set; }
        [field: SerializeField] public int StartingAmount { get; private set; }
    }
}
