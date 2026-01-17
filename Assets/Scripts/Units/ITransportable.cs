using UnityEngine;
using UnityEngine.AI;

public interface ITransportable
{
    public Transform Transform { get; }
    public int TransportCapacityUsage { get; }
    public NavMeshAgent Agent { get; }
    public Sprite Icon { get; }
    public Owner Owner { get; }
    public void LoadInto(ITransporter transporter);
}
