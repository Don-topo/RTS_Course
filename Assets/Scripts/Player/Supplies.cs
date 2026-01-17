using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Supplies : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mineralsText;
    [SerializeField] private TextMeshProUGUI gasText;
    [SerializeField] private TextMeshProUGUI populationText;

    [SerializeField] private SupplySO mineralsSO;
    [SerializeField] private SupplySO gasSO;

    public static Dictionary<Owner, int> Minerals {  get; private set; }
    public static Dictionary<Owner, int> Gas { get; private set; }
    public static Dictionary<Owner, int> Population { get; private set; }
    public static Dictionary<Owner, int> PopulationLimit { get; private set; }

    private static readonly string POPULATION_TEXT_FORMAT = "{0} / {1}";
    private static readonly string ERROR_POPULATION_TEXT_FORMAT = "<color=#ac0000>{0}</color> / {1}";

    private void Awake()
    {
        Minerals = new Dictionary<Owner, int>();
        Gas = new Dictionary<Owner, int>();
        Population = new Dictionary<Owner, int>();
        PopulationLimit = new Dictionary<Owner, int>();

        foreach(Owner owner in Enum.GetValues(typeof(Owner)))
        {
            Minerals.Add(owner, 0);
            Gas.Add(owner, 0);
            Population.Add(owner, 0);
            PopulationLimit.Add(owner, 0);
        }
        Bus<SupplyEvent>.RegisterForAll(HandleSupplyEvent);
        Bus<PopulationEvent>.RegisterForAll(HandlePopulationEvent);
    }

    private void OnDestroy()
    {
        Bus<SupplyEvent>.UnregisterForAll(HandleSupplyEvent);
        Bus<PopulationEvent>.UnregisterForAll(HandlePopulationEvent);
    }

    private void HandleSupplyEvent(SupplyEvent evt)
    {
        if (evt.Supply.Equals(mineralsSO))
        {
            Minerals[evt.Owner] += evt.Amount;
            if(Owner.Player1 == evt.Owner)
            {
                mineralsText.SetText(Minerals[evt.Owner].ToString());
            }            
        }
        else if (evt.Supply.Equals(gasSO))
        {
            Gas[evt.Owner] += evt.Amount;
            if(Owner.Player1 == evt.Owner)
            {
                gasText.SetText(Gas[evt.Owner].ToString());
            }            
        }
    }

    private void HandlePopulationEvent(PopulationEvent evt)
    {
        Population[evt.Owner] += evt.PopulationChange;
        PopulationLimit[evt.Owner] += evt.PopulationLimitChange;

        if(Owner.Player1 == evt.Owner)
        {
            int currentPopulation = Population[evt.Owner];
            int maxPopulation = PopulationLimit[evt.Owner];

            if(currentPopulation <= maxPopulation)
            {
                populationText.SetText(string.Format(POPULATION_TEXT_FORMAT, currentPopulation, maxPopulation));
            }
            else
            {
                populationText.SetText(string.Format(ERROR_POPULATION_TEXT_FORMAT, currentPopulation, maxPopulation));
            }
        }
    }
}
