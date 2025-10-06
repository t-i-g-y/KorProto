using UnityEngine;

[CreateAssetMenu(menuName="RailProto/Station Tier")]
public class StationTier : ScriptableObject 
{
    public string displayName = "Tier III";
    [Min(0)] public int baseValue = 50;
    [Min(0f)] public float cargoValueMultiplier = 1f;
}

