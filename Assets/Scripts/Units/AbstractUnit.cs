using NUnit.Framework;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(NavMeshAgent), typeof(BehaviorGraphAgent))]
public abstract class AbstractUnit : AbstractCommandable, IMoveable, IAttacker
{    
    public float AgentRadius => Agent.radius;
    [field: SerializeField] public ParticleSystem AttackingParticleSystem { get; private set; }
    [SerializeField] private DamageableSensor damageableSensor;
    public NavMeshAgent Agent {get; private set;}
    public Sprite Icon => UnitSO.Icon;
    protected BehaviorGraphAgent graphAgent;
    protected UnitSO unitSO;

    protected override void Start()
    {
        base.Start();
        CurrentHealth = UnitSO.Health;
        MaxHealth = UnitSO.Health;
        Bus<UnitSpawnEvent>.Raise(Owner, new UnitSpawnEvent(this));

        if(damageableSensor != null)
        {
            damageableSensor.OnUnitEnter += HandleUnitEnter;
            damageableSensor.OnUnitExit += HandleUnitExit;
            damageableSensor.Owner = Owner;
            damageableSensor.SetupFrom(unitSO.AttackConfig);
        }

        foreach(UpgradeSO upgrade in unitSO.Upgrades)
        {
            if(unitSO.TechTree.IsResearched(Owner, upgrade))
            {
                upgrade.Apply(unitSO);
            }            
        }

        Bus<PopulationEvent>.Raise(Owner, new PopulationEvent(
            Owner,
            0,
            unitSO.PopulationConfig.PopulationSupply
        ));
    }

    protected override void Awake()
    {
        base.Awake();
        Agent = GetComponent<NavMeshAgent>();
        graphAgent = GetComponent<BehaviorGraphAgent>();
        unitSO = UnitSO as UnitSO;
        graphAgent.SetVariableValue("Command", UnitCommands.Stop);
        graphAgent.SetVariableValue("AttackConfig", unitSO.AttackConfig);
    }

    protected override void OnDestroy()
    {
        Bus<UnitDeathEvent>.Raise(Owner, new UnitDeathEvent(this));
    }

    public void MoveTo(Vector3 position)
    {
        SetCommandsOverrides(null);
        graphAgent.SetVariableValue("TargetLocation", position);
        graphAgent.SetVariableValue<GameObject>("TargetGameObject", null);
        graphAgent.SetVariableValue("Command", UnitCommands.Move);
    }

    public void Stop()
    {
        SetCommandsOverrides(null);
        graphAgent.SetVariableValue("Command", UnitCommands.Stop);
    }

    private void HandleUnitEnter(IDamageable damageable)
    {
        List<GameObject> nearbyEnemies = SetNearbyEnemiesOnBlackboard();

        if(graphAgent.GetVariable("TargetGameObject", out BlackboardVariable<GameObject> targetVariable)
            && targetVariable.Value == null && nearbyEnemies.Count > 0)
        {
            graphAgent.SetVariableValue("TargetGameObject", nearbyEnemies[0]);
        }
    }

    private void HandleUnitExit(IDamageable damageable)
    {
        List<GameObject> nearbyEnemies = SetNearbyEnemiesOnBlackboard();

        if(!graphAgent.GetVariable("TargetGameObject", out BlackboardVariable<GameObject> targetVariable)
            || damageable.Transform.gameObject != targetVariable.Value) return;

        if(nearbyEnemies.Count > 0)
        {
            graphAgent.SetVariableValue("TargetGameObject", nearbyEnemies[0]);
        }
        else
        {
            graphAgent.SetVariableValue<GameObject>("TargetGameObject", null);
            graphAgent.SetVariableValue("TargetLocation", damageable.Transform.position);
        }
    }

    private List<GameObject> SetNearbyEnemiesOnBlackboard()
    {
        List<GameObject> nearbyEnemies = damageableSensor.Damageables.ConvertAll(
                    damage => damage.Transform.gameObject);
        nearbyEnemies.Sort(new ClosestGameObjectComparer(transform.position));

        graphAgent.SetVariableValue("NearbyEnemies", nearbyEnemies);

        return nearbyEnemies;
    }

    public void Attack(IDamageable damageable)
    {
        graphAgent.SetVariableValue("TargetGameObject", damageable.Transform.gameObject);
        graphAgent.SetVariableValue("Command", UnitCommands.Attack);
    }

    public void Attack(Vector3 location)
    {
        graphAgent.SetVariableValue<GameObject>("TargetGameObject", null);
        graphAgent.SetVariableValue("TargetLocation", location);
        graphAgent.SetVariableValue("Command", UnitCommands.Attack);
    }

    public void MoveTo(Transform transform)
    {
        SetCommandsOverrides(null);
        graphAgent.SetVariableValue("TargetGameObject", transform.gameObject);
        graphAgent.SetVariableValue("Command", UnitCommands.Move);
    }
}
