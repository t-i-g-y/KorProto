using System.Collections.Generic;
using UnityEngine;

public class RailEconomySystem : MonoBehaviour
{
    public static RailEconomySystem Instance { get; private set; }

    [SerializeField] private EconomyConfig config;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float CalculateLineConstructionCost(RailLine line)
    {
        if (line == null || config == null)
            return 0f;

        float total = 0f;

        foreach (Vector3Int cell in line.Cells)
        {
            TerrainType terrain = HexRailNetwork.Instance.GetTerrainType(cell);
            float modifier = config.GetConstructionModifier(terrain);

            if (ResearchModifierSystem.Instance != null)
                modifier *= ResearchModifierSystem.Instance.GetTerrainConstructionMultiplier(terrain);

            total += config.BaseConstructionCostPerCell * modifier;
        }

        return total;
    }

    public float CalculateLineRefund(RailLine line)
    {
        if (line == null || config == null)
            return 0f;

        return CalculateLineConstructionCost(line) * config.RefundRatio;
    }

    public float CalculateLineIncome(RailLine line)
    {
        if (line == null || config == null)
            return 0f;

        if (!StationRegistry.TryGet(line.Start, out Station startStation))
            return 0f;

        if (!StationRegistry.TryGet(line.End, out Station endStation))
            return 0f;

        float total = 0f;

        IReadOnlyList<StationAttribute> startAttributes = startStation.Attributes;
        IReadOnlyList<StationAttribute> endAttributes = endStation.Attributes;

        foreach (StationAttribute a in startAttributes)
        {
            foreach (StationAttribute b in endAttributes)
            {
                total += config.GetAttributeRelationValue(a.AttributeType, b.AttributeType);
            }
        }

        if (ResearchModifierSystem.Instance != null)
            total *= ResearchModifierSystem.Instance.RailConnectionIncomeResearchMultiplier;

        Debug.Log($"Rail profit for {line.ID}: {total}");
        return total;
    }

    public float ApplyLineIncome(RailLine line)
    {
        if (line == null || FinanceSystem.Instance == null)
            return 0f;

        float income = CalculateLineIncome(line);

        if (income > 0f)
            FinanceSystem.Instance.AdjustBalance(income);

        return income;
    }

    public float CalculateLineMaintenance(RailLine line)
    {
        if (line == null || config == null)
            return 0f;

        float total = config.BaseLineMaintenanceFlat;

        foreach (Vector3Int cell in line.Cells)
        {
            TerrainType terrain = HexRailNetwork.Instance.GetTerrainType(cell);
            float modifier = config.GetMaintenanceModifier(terrain);

            if (ResearchModifierSystem.Instance != null)
                modifier *= ResearchModifierSystem.Instance.GetTerrainRailMaintenanceMultiplier(terrain);

            total += config.BaseLineMaintenancePerCell * modifier;
        }

        return total;
    }

    public float CalculateTrainMaintenance(Train train)
    {
        if (train == null || config == null)
            return 0f;

        float trainMaintenance = config.BaseTrainMaintenanceFlat;
        if (ResearchModifierSystem.Instance != null)
            trainMaintenance *= ResearchModifierSystem.Instance.TrainMaintenanceResearchMultiplier;

        return trainMaintenance;
    }

    public float CalculateTotalMaintenance()
    {
        float total = 0f;

        if (RailManager.Instance != null)
        {
            foreach (RailLine line in RailManager.Instance.Lines)
                total += CalculateLineMaintenance(line);
        }

        if (TrainManager.Instance != null)
        {
            foreach (Train train in TrainManager.Instance.Trains)
                total += CalculateTrainMaintenance(train);
        }

        return total;
    }

    public void HandleLineCreated(RailLine line)
    {
        float constructionCost = CalculateLineConstructionCost(line);
        FinanceSystem.Instance.PayConstruction(constructionCost);
    }

    public void HandleLineRemoved(RailLine line)
    {
        float refund = CalculateLineRefund(line);
        FinanceSystem.Instance.ApplyRefund(refund);
    }

    public void ApplyRailEconomyTick()
    {
        float maintenance = CalculateTotalMaintenance();

        if (maintenance > 0f)
            FinanceSystem.Instance.DeductMaintenanceCost(maintenance);
    }

    private void OnEnable()
    {
        RailManager.LineCreated += HandleLineCreated;
        RailManager.LineRemoved += HandleLineRemoved;
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        RailManager.LineRemoved -= HandleLineRemoved;
    }

}
