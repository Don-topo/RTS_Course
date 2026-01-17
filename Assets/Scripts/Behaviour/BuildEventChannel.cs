using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Build Event Channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Build Event Channel", message: "[Self] [BuildingEventType] on [BaseBuilding] .", category: "Events", id: "03510107194c9a0e180acffb628c309d")]
public sealed partial class BuildEventChannel : EventChannel<GameObject, BuildingEventType, BaseBuilding> { }

