using UnityEngine;
using System.Collections.Generic;
using TMPro;

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
    public Color techColor;
}

public enum TechID
{
    None,

    BaseTerrainModifiers,
    BridgeUnlock,
    TundraUnlock,
    TerrainMaintenance,
    DesertTropicsUnlock,
    TerrainConstruction,
    MountainTunnel,
    SeaTunnel,
    TerrainMaintenanceConstruction,

    BaseRailMaintenance,
    RailConnectionIncome,
    BaseRelayMaintenance,
    BaseRailAndRelayMaintenance,
    RailConnectionIncome2,
    RailConnectionIncome3,
    BaseRailMaintenanceAndIncome,

    BaseTrainWagonMaintenance,
    BaseTrainCost,
    BaseTrainWagonMaintenance2,
    UpgradeTiers,
    WagonCapacity,
    AllSpeedUpgrade,
    UpgradeTiers2,
    WagonUpgrade,
    BaseTrainWagonCostAndMaintenance,

    GlobalResearch,
    GlobalLocalResearch,
    LocalResearch,
    LocalResearch2,
    GlobalLocalResearch2
}
