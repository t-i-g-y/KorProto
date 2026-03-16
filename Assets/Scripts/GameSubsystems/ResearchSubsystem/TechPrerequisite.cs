using UnityEngine;

[CreateAssetMenu(fileName = "TechPrerequisite", menuName = "Research/TechPrerequisite")]
public class TechPrerequisite : ScriptableObject
{
    public TechData[] requiredTech;
    public int requiredBalance;
    public int connectStations;
    public int deliveredCargo;
    public TerrainType[] requiredTerrains;
}
