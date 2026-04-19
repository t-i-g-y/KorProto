using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Tech", menuName = "Research/TechData")]
public class TechData : ScriptableObject
{
    public TechID ID;
    public string techName;
    public string techDescription;
    public Sprite techImage;
    public int researchCost;
    public List<TechID> prerequisites = new();
    public Vector2 technologyTreePos;
}

public enum TechID
{
    None,

    GrasslandModifier,
    FreshwaterBridge,
    MountainTunnel,
    SeaTunnel,
    RailMaintenance,
    TrainWagonMaintenance,
    TrainSpeed,
    CapacityUpgrade,
    CapacityUpgrade2,
    LoadUnloadSpeed,
    RelayCapacity,
    RailConnectionIncome,
    LoadSpeedLocalResearch,
    CargoSaleIncome,
    RailConnectionIncome2,
    AllMaintenance,
    LocalResearch,
    GlobalResearch
}
