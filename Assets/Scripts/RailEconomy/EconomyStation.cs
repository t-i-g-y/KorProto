using UnityEngine;

public class EconomyStation : MonoBehaviour 
{
    public StationTier tier;

    [Header("Cargo generation (very simple)")]
    [Range(0.5f, 2f)] public float randomSpread = 0.2f;    public int CurrentCargoValue() {
        var baseVal = tier ? tier.baseValue : 50;
        float r = 1f + Random.Range(-randomSpread, randomSpread);
        return Mathf.RoundToInt(baseVal * r);
    }
}
