using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Load Unit Event Channel")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Load Unit Event Channel", message: "[Self] loads [TargetGameObject] into itself .", category: "Events", id: "be207e1131a3466c58efb059f4786e06")]
public sealed partial class LoadUnitEventChannel : EventChannel<GameObject, GameObject> { }

