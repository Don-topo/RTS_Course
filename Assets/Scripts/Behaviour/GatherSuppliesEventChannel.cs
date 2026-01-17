using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/GatherSuppliesEventChannel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "GatherSuppliesEventChannel", message: "[Self] gathers [Amount] [Supplies] .", category: "Events", id: "906aa53d490d15158a69a7d7b5773e6f")]
public sealed partial class GatherSuppliesEventChannel : EventChannel<GameObject, int, SupplySO> { }

