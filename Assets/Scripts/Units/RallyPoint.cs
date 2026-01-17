using UnityEngine;

[System.Serializable]
public struct RallyPoint
{
    [field: SerializeField] public bool IsSet { get; private set; }
    [field: SerializeField] public Vector3 Point { get; private set; }
    [field: SerializeField] public GameObject Target { get; private set; }

    public RallyPoint(bool isSet, Vector3 point, GameObject target)
    {
        IsSet = isSet;
        Point = point;
        Target = target;
    }
}
