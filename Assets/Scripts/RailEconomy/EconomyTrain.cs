using UnityEngine;

[CreateAssetMenu(menuName="RailProto/Train")]
public class EconomyTrain : ScriptableObject {
    public string displayName = "Train";
    [Min(0)] public int capacity = 10;
    [Min(0f)] public float speedTilesPerSecond = 1.0f;
    [Min(0)] public int maintenancePerTick = 5;
}

