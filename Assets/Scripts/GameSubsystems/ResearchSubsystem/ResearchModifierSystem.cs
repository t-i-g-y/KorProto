using System.Collections.Generic;
using UnityEngine;

public class ResearchModifierSystem : MonoBehaviour
{
    public static ResearchModifierSystem Instance { get; private set; }

    public float RailMaintenanceResearchMultiplier { get; private set; } = 1f;
    public float TrainMaintenanceResearchMultiplier { get; private set; } = 1f;
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
    public int CargoCapacityBonus { get; private set; } = 0;
    public int WagonUpgradeTiers { get; private set; } = 0;
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

    private void ResetBuildableTerrains()
    {
        buildableTerrains.Clear();
        buildableTerrains.Add(TerrainType.Grassland);
        buildableTerrains.Add(TerrainType.Forest);
        buildableTerrains.Add(TerrainType.Hills);
    }

    public void ResetModifiers()
    {
        RailMaintenanceResearchMultiplier = 1f;
        TrainMaintenanceResearchMultiplier = 1f;
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
        CargoCapacityBonus = 0;
        WagonUpgradeTiers = 0;

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
            case TechID.GrasslandModifier:
                terrainConstructionResearchMultipliers[TerrainType.Grassland] = 0.9f;
                break;

            case TechID.FreshwaterBridge:
                buildableTerrains.Add(TerrainType.Lake);
                terrainConstructionResearchMultipliers[TerrainType.Lake] = 0.85f;
                break;

            case TechID.MountainTunnel:
                buildableTerrains.Add(TerrainType.Mountain);
                terrainRailMaintenanceResearchMultipliers[TerrainType.Mountain] = 0.85f;
                break;

            case TechID.SeaTunnel:
                buildableTerrains.Add(TerrainType.Sea);
                terrainConstructionResearchMultipliers[TerrainType.Sea] = 0.8f;
                break;

            case TechID.RailMaintenance:
                RailMaintenanceResearchMultiplier *= 0.85f;
                break;

            case TechID.TrainWagonMaintenance:
                TrainMaintenanceResearchMultiplier *= 0.9f;
                WagonMaintenanceResearchMultiplier *= 0.9f;
                break;

            case TechID.TrainSpeed:
                TrainSpeedResearchMultiplier *= 1.15f;
                break;

            case TechID.CapacityUpgrade:
                CargoCapacityResearchMultiplier *= 1.2f;
                WagonUpgradeTiers += 1;
                break;

            case TechID.CapacityUpgrade2:
                CargoCapacityResearchMultiplier *= 1.35f;
                break;
            
            case TechID.LoadUnloadSpeed:
                CargoLoadSpeedResearchMultiplier *= 1.25f;
                CargoUnloadSpeedResearchMultiplier *= 1.25f;
                break;

            case TechID.RelayCapacity:
                terrainRelayCapacityResearchMultipliers[TerrainType.Grassland] = 1.3f;
                RelayMaintenanceResearchMultiplier *= 0.9f;
                break;

            case TechID.RailConnectionIncome:
                RailConnectionIncomeResearchMultiplier *= 1.15f;
                break;

            case TechID.LoadSpeedLocalResearch:
                CargoLoadSpeedResearchMultiplier *= 1.15f;
                LocalResearchIncomeMultiplier *= 1.1f;
                break;
            
            case TechID.CargoSaleIncome:
                CargoSaleIncomeResearchMultiplier *= 1.15f;
                break;

            case TechID.RailConnectionIncome2:
                RailConnectionIncomeResearchMultiplier *= 1.2f;
                break;

            case TechID.AllMaintenance:
                RailMaintenanceResearchMultiplier *= 0.9f;
                TrainMaintenanceResearchMultiplier *= 0.9f;
                RelayMaintenanceResearchMultiplier *= 0.9f;
                break;
            
            case TechID.LocalResearch:
                LocalResearchIncomeMultiplier *= 1.25f;
                break;

            case TechID.GlobalResearch:
                GlobalResearchIncomeMultiplier *= 1.25f;
                break;
        }
    }

    public bool CanBuildOn(TerrainType terrain) => buildableTerrains.Contains(terrain);

    public float GetTerrainConstructionMultiplier(TerrainType terrain) => terrainConstructionResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;

    public float GetTerrainRailMaintenanceMultiplier(TerrainType terrain) => terrainRailMaintenanceResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;

    public float GetTerrainRelayMaintenanceMultiplier(TerrainType terrain) => terrainRelayMaintenanceResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;

    public float GetTerrainRelayCapacityMultiplier(TerrainType terrain) => terrainRelayCapacityResearchMultipliers.TryGetValue(terrain, out float value) ? value : 1f;
}
