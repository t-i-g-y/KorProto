using System.Collections.Generic;
using UnityEngine;

public class ResearchModifierSystem : MonoBehaviour
{
    public static ResearchModifierSystem Instance { get; private set; }

    public float RailMaintenanceResearchMultiplier { get; private set; } = 1f;
    public float TrainMaintenanceResearchMultiplier { get; private set; } = 1f;
    public float TrainCostResearchMultiplier { get; private set; } = 1f;
    public float TrainBreakChanceMultiplier { get; private set; } = 1f;
    public float WagonCostResearchMultiplier { get; private set; } = 1f;
    public float RelayMaintenanceResearchMultiplier { get; private set; } = 1f;
    public float WagonMaintenanceResearchMultiplier { get; private set; } = 1f;
    public float TrainSpeedResearchMultiplier { get; private set; } = 1f;
    public float CargoLoadSpeedResearchMultiplier { get; private set; } = 1f;
    public float CargoUnloadSpeedResearchMultiplier { get; private set; } = 1f;
    public float CargoSaleIncomeResearchMultiplier { get; private set; } = 1f;
    public float RailConnectionIncomeResearchMultiplier { get; private set; } = 1f;
    public float GlobalResearchIncomeMultiplier { get; private set; } = 1f;
    public float LocalResearchIncomeMultiplier { get; private set; } = 1f;
    public float CargoCapacityResearchMultiplier { get; private set; } = 1f;
    public int WagonCargoCapacityBonus { get; private set; } = 0;
    public int LocomotiveCargoCapacityBonus { get; private set; } = 0;
    public int WagonUpgradeTiers { get; private set; } = 0;
    public int SpeedUpgradeTiers { get; private set; } = 0;
    public int RailLengthBonus {get; private set; } = 0;
    public int TrainPerLineBonus { get; private set; } = 0;
    private HashSet<TerrainType> buildableTerrains = new();
    private Dictionary<TerrainType, float> terrainConstructionResearchMultipliers = new();
    private Dictionary<TerrainType, float> terrainRailMaintenanceResearchMultipliers = new();
    private Dictionary<TerrainType, float> terrainRelayMaintenanceResearchMultipliers = new();
    private Dictionary<TerrainType, float> terrainRelayCapacityResearchMultipliers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetModifiers();
    }

    private void AddBuildableTerrain(TerrainType terrain)
    {
        buildableTerrains.Add(terrain);
        terrainConstructionResearchMultipliers[terrain] = 1f;
        terrainRailMaintenanceResearchMultipliers[terrain] = 1f;
        terrainRelayMaintenanceResearchMultipliers[terrain] = 1f;
    }
    private void ResetBuildableTerrains()
    {
        buildableTerrains.Clear();
        AddBuildableTerrain(TerrainType.Grassland);
        AddBuildableTerrain(TerrainType.Forest);
        AddBuildableTerrain(TerrainType.Hills);
    }

    private void ApplyTerrainConstructionModifiers(float modifier)
    {
        foreach (TerrainType terrain in buildableTerrains)
        {
            EnsureTerrainEntry(terrain);
            terrainConstructionResearchMultipliers[terrain] -= modifier;
        }
    }

    private void ApplyTerrainConstructionModifiers(float modifier, TerrainType[] terrains)
    {
        foreach (TerrainType terrain in terrains)
        {
            EnsureTerrainEntry(terrain);
            terrainConstructionResearchMultipliers[terrain] -= modifier;
        }
    }

    private void ApplyTerrainRailMaintenanceModifiers(float modifier)
    {
        foreach (TerrainType terrain in buildableTerrains)
        {
            EnsureTerrainEntry(terrain);
            terrainRailMaintenanceResearchMultipliers[terrain] -= modifier;
        }
    }

    private void ApplyTerrainRailMaintenanceModifiers(float modifier, TerrainType[] terrains)
    {
        foreach (TerrainType terrain in terrains)
        {
            EnsureTerrainEntry(terrain);
            terrainRailMaintenanceResearchMultipliers[terrain] -= modifier;
        }
    }

    private void ApplyTerrainRelayMaintenanceModifiers(float modifier)
    {
        foreach (TerrainType terrian in buildableTerrains)
        {
            EnsureTerrainEntry(terrian);
            terrainRelayMaintenanceResearchMultipliers[terrian] -= modifier;
        }
    }

    private void ApplyTerrainRelayMaintenanceModifiers(float modifier, TerrainType[] terrains)
    {
        foreach (TerrainType terrian in terrains)
        {
            EnsureTerrainEntry(terrian);
            terrainRelayMaintenanceResearchMultipliers[terrian] -= modifier;
        }
    }

    private void EnsureTerrainEntry(TerrainType terrain)
    {
        if (!terrainConstructionResearchMultipliers.ContainsKey(terrain))
            terrainConstructionResearchMultipliers[terrain] = 1f;

        if (!terrainRailMaintenanceResearchMultipliers.ContainsKey(terrain))
            terrainRailMaintenanceResearchMultipliers[terrain] = 1f;

        if (!terrainRelayMaintenanceResearchMultipliers.ContainsKey(terrain))
            terrainRelayMaintenanceResearchMultipliers[terrain] = 1f;
    }

    public void ResetModifiers()
    {
        RailMaintenanceResearchMultiplier = 1f;
        TrainMaintenanceResearchMultiplier = 1f;
        TrainCostResearchMultiplier = 1f;
        TrainBreakChanceMultiplier = 1f;
        WagonCostResearchMultiplier = 1f;
        RelayMaintenanceResearchMultiplier = 1f;
        WagonMaintenanceResearchMultiplier = 1f;
        TrainSpeedResearchMultiplier = 1f;
        CargoLoadSpeedResearchMultiplier = 1f;
        CargoUnloadSpeedResearchMultiplier = 1f;
        CargoSaleIncomeResearchMultiplier = 1f;
        RailConnectionIncomeResearchMultiplier = 1f;
        GlobalResearchIncomeMultiplier = 1f;
        LocalResearchIncomeMultiplier = 1f;
        CargoCapacityResearchMultiplier = 1f;
        LocomotiveCargoCapacityBonus = 0;
        WagonCargoCapacityBonus = 0;
        RailLengthBonus = 0;
        WagonUpgradeTiers = 1;
        SpeedUpgradeTiers = 1;

        ResetBuildableTerrains();
        terrainConstructionResearchMultipliers.Clear();
        terrainRailMaintenanceResearchMultipliers.Clear();
        terrainRelayMaintenanceResearchMultipliers.Clear();
        terrainRelayCapacityResearchMultipliers.Clear();
    }

    public void RebuildFromResearch(ResearchSystem researchSystem)
    {
        ResetModifiers();

        if (researchSystem == null)
            return;

        foreach (var pair in researchSystem.GetAllTechnologies())
        {
            Technology tech = pair.Value;
            if (tech != null && tech.IsUnlocked && tech.Data != null)
                ApplyTechnology(tech.Data.ID);
        }
    }

    public void ApplyTechnology(TechID ID)
    {
        switch (ID)
        {
            case TechID.BaseTerrainModifiers:
                ApplyTerrainConstructionModifiers(0.1f);
                ApplyTerrainRailMaintenanceModifiers(0.05f);
                ApplyTerrainRelayMaintenanceModifiers(0.05f);
                break;

            case TechID.BridgeUnlock:
                AddBuildableTerrain(TerrainType.Swamp);
                AddBuildableTerrain(TerrainType.Lake);
                AddBuildableTerrain(TerrainType.River);
                break;

            case TechID.TundraUnlock:
                buildableTerrains.Add(TerrainType.Tundra);
                TerrainType[] tundraUnlockTerrains = new TerrainType[] {TerrainType.Hills, TerrainType.Forest};
                ApplyTerrainConstructionModifiers(0.05f, tundraUnlockTerrains);
                ApplyTerrainRailMaintenanceModifiers(0.05f, tundraUnlockTerrains);
                ApplyTerrainRelayMaintenanceModifiers(0.05f, tundraUnlockTerrains);
                break;

            case TechID.TerrainMaintenance:
                ApplyTerrainRailMaintenanceModifiers(0.1f);
                ApplyTerrainRelayMaintenanceModifiers(0.1f);
                break;

            case TechID.DesertTropicsUnlock:
                AddBuildableTerrain(TerrainType.Desert);
                AddBuildableTerrain(TerrainType.Tropics);
                TerrainType[] grasslandTerrain = new TerrainType[] {TerrainType.Grassland};
                ApplyTerrainConstructionModifiers(0.1f, grasslandTerrain);
                ApplyTerrainRailMaintenanceModifiers(0.1f, grasslandTerrain);
                ApplyTerrainRelayMaintenanceModifiers(0.1f, grasslandTerrain);
                break;
            
            case TechID.TerrainConstruction:
                TerrainType[] terrainConstructionTerrains = new TerrainType[] {TerrainType.Forest, TerrainType.Swamp, TerrainType.Lake, TerrainType.River, TerrainType.Tundra, TerrainType.Tropics};
                ApplyTerrainConstructionModifiers(0.1f, terrainConstructionTerrains);
                break;

            case TechID.MountainTunnel:
                AddBuildableTerrain(TerrainType.Mountain);
                break;

            case TechID.SeaTunnel:
                AddBuildableTerrain(TerrainType.Sea);
                TerrainType[] seaTunnelTerrains = new TerrainType[] {TerrainType.Swamp, TerrainType.Lake, TerrainType.River};
                ApplyTerrainConstructionModifiers(0.05f, seaTunnelTerrains);
                break;

            case TechID.TerrainMaintenanceConstruction:
                ApplyTerrainConstructionModifiers(0.1f);
                ApplyTerrainRailMaintenanceModifiers(0.1f);
                ApplyTerrainRelayMaintenanceModifiers(0.1f);
                break;
            

            case TechID.BaseRailMaintenance:
                RailMaintenanceResearchMultiplier -= 0.1f;
                break;
            
            case TechID.RailConnectionIncome:
                RailConnectionIncomeResearchMultiplier += 0.2f;
                break;
            
            case TechID.BaseRelayMaintenance:
                RelayMaintenanceResearchMultiplier -= 0.1f;
                RailLengthBonus += 2;
                break;
            
            case TechID.BaseRailAndRelayMaintenance:
                RailMaintenanceResearchMultiplier -= 0.05f;
                RelayMaintenanceResearchMultiplier -= 0.05f;
                break;
            
            case TechID.RailConnectionIncome2:
                RailConnectionIncomeResearchMultiplier += 0.15f;
                CargoSaleIncomeResearchMultiplier += 0.15f;
                break;
            
            case TechID.RailConnectionIncome3:
                RailConnectionIncomeResearchMultiplier += 0.25f;
                CargoSaleIncomeResearchMultiplier += 0.25f;
                RelayMaintenanceResearchMultiplier -= 0.1f;
                break;
            
            case TechID.BaseRailMaintenanceAndIncome:
                RailMaintenanceResearchMultiplier -= 0.1f;
                RelayMaintenanceResearchMultiplier -= 0.1f;
                RailConnectionIncomeResearchMultiplier += 0.15f;
                RailLengthBonus += 2;
                break;
            

            case TechID.BaseTrainWagonMaintenance:
                TrainMaintenanceResearchMultiplier -= 0.05f;
                WagonMaintenanceResearchMultiplier -= 0.05f;
                break;
            
            case TechID.BaseTrainCost:
                TrainCostResearchMultiplier -= 0.1f;
                WagonCostResearchMultiplier -= 0.1f;
                TrainSpeedResearchMultiplier += 0.1f;
                RailLengthBonus += 1;
                break;
            
            case TechID.BaseTrainWagonMaintenance2:
                TrainMaintenanceResearchMultiplier -= 0.05f;
                WagonMaintenanceResearchMultiplier -= 0.05f;
                CargoLoadSpeedResearchMultiplier -= 0.1f;
                CargoUnloadSpeedResearchMultiplier -= 0.1f;
                break;

            case TechID.UpgradeTiers:
                WagonUpgradeTiers += 1;
                SpeedUpgradeTiers += 1;
                TrainCostResearchMultiplier -= 0.05f;
                break;
            
            case TechID.WagonCapacity:
                WagonCargoCapacityBonus += 2;
                WagonCostResearchMultiplier -= 0.05f;
                break;
            
            case TechID.AllSpeedUpgrade:
                TrainSpeedResearchMultiplier += 0.2f;
                CargoLoadSpeedResearchMultiplier -= 0.2f;
                CargoUnloadSpeedResearchMultiplier -= 0.2f;
                RelayMaintenanceResearchMultiplier -= 0.1f;
                break;

            case TechID.UpgradeTiers2:
                WagonUpgradeTiers += 2;
                SpeedUpgradeTiers += 1;
                LocomotiveCargoCapacityBonus += 2;
                break;
            
            case TechID.WagonUpgrade:
                WagonUpgradeTiers += 2;
                WagonCargoCapacityBonus += 2;
                RailLengthBonus += 1;
                break;

            case TechID.BaseTrainWagonCostAndMaintenance:
                TrainCostResearchMultiplier -= 0.1f;
                WagonCostResearchMultiplier -= 0.1f;
                TrainMaintenanceResearchMultiplier -= 0.1f;
                WagonMaintenanceResearchMultiplier -= 0.1f;
                TrainSpeedResearchMultiplier += 0.15f;
                CargoLoadSpeedResearchMultiplier -= 0.1f;
                CargoUnloadSpeedResearchMultiplier -= 0.1f;
                break;

            
            case TechID.GlobalResearch:
                GlobalResearchIncomeMultiplier += 0.1f;
                break;
            
            case TechID.GlobalLocalResearch:
                GlobalResearchIncomeMultiplier += 0.15f;
                LocalResearchIncomeMultiplier += 0.15f;
                TrainCostResearchMultiplier -= 0.05f;
                break;

            case TechID.LocalResearch:
                LocalResearchIncomeMultiplier += 0.1f;
                ApplyTerrainConstructionModifiers(0.05f);
                ApplyTerrainRailMaintenanceModifiers(0.05f);
                ApplyTerrainRelayMaintenanceModifiers(0.05f);
                break;

            case TechID.LocalResearch2:
                LocalResearchIncomeMultiplier += 0.25f;
                break;

            case TechID.GlobalLocalResearch2:
                GlobalResearchIncomeMultiplier += 0.5f;
                LocalResearchIncomeMultiplier += 0.1f;
                break;
        }
    }

    public bool CanBuildOn(TerrainType terrain) => buildableTerrains.Contains(terrain);

    public float GetTerrainConstructionMultiplier(TerrainType terrain) => terrainConstructionResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;

    public float GetTerrainRailMaintenanceMultiplier(TerrainType terrain) => terrainRailMaintenanceResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;

    public float GetTerrainRelayMaintenanceMultiplier(TerrainType terrain) => terrainRelayMaintenanceResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;

    public float GetTerrainRelayCapacityMultiplier(TerrainType terrain) => terrainRelayCapacityResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;
}
