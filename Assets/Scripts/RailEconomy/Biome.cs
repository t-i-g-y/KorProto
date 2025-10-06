using UnityEngine;

[CreateAssetMenu(menuName="RailProto/Biome")]
public class Biome : ScriptableObject 
{
    public string displayName = "Grassland";
    [Min(0f)] public float buildCostMultiplier = 1f;
    [Min(0f)] public float upkeepMultiplier = 1f;
}

