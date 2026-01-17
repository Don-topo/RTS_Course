using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit")]
public class UnitSO : AbstractUnitSO
{
    [field: SerializeField] public AttackConfigSO AttackConfig { get; private set; }
    [field: SerializeField] public TransportConfigSO TransportConfig { get; private set; }

    public override object Clone()
    {
        UnitSO copy = base.Clone() as UnitSO;

        copy.AttackConfig = AttackConfig == null ? null : Instantiate(AttackConfig);
        copy.TransportConfig = TransportConfig == null ? null : Instantiate(TransportConfig);
        copy.SightConfig = SightConfig == null ? null : Instantiate(SightConfig);
        copy.PopulationConfig = PopulationConfig == null ? null : Instantiate(PopulationConfig);

        return copy;
    }
}
