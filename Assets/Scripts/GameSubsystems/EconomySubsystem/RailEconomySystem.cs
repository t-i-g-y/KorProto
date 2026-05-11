using System.Collections.Generic;
using UnityEngine;

public class RailEconomySystem : MonoBehaviour
{
    public static RailEconomySystem Instance { get; private set; }

    public List<RailIncomeToken> trackedIncomeTokens = new();
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

    public float CalculateLineConstructionCost(List<Vector3Int> cells)
    {
        float total = 0f;

        foreach (Vector3Int cell in cells)
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

    public float CalculateLineIncome(Station stationA, Station stationB)
    {
        float total = 0f;

        IReadOnlyList<StationAttribute> startAttributes = stationA.Attributes;
        IReadOnlyList<StationAttribute> endAttributes = stationB.Attributes;

        foreach (StationAttribute a in startAttributes)
        {
            foreach (StationAttribute b in endAttributes)
            {
                total += config.GetAttributeRelationValue(a.AttributeType, b.AttributeType);
            }
        }

        if (ResearchModifierSystem.Instance != null)
            total *= ResearchModifierSystem.Instance.RailConnectionIncomeResearchMultiplier;

        Debug.Log($"Rail profit for {stationA.StationID}<->{stationB.StationID}: {total}");
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

    public float ApplyTransitLineIncome(RailLine line, Vector3Int cell)
    {
        if (line == null || FinanceSystem.Instance == null)
            return 0f;

        if (IsDirectStationToStationLine(line))
            return 0f;

        float income = AdvanceIncomeTokens(line, cell);

        TryCreateIncomeTokens(line, cell);

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
        total *= ResearchModifierSystem.Instance.RailMaintenanceResearchMultiplier;
        return total;
    }

    public float CalculateRelayMaintenance()
    {
        if (config == null)
            return 0f;
        
        float total = 0f;

        foreach (RelayStop relay in RelayStopRegistry.Instance.Relays.Values)
        {
            float maintenance = config.BaseRelayMaintenance;
            TerrainType terrain = HexRailNetwork.Instance.GetTerrainType(relay.Cell);
            float modifier = config.GetMaintenanceModifier(terrain);

            if (ResearchModifierSystem.Instance != null)
                modifier *= ResearchModifierSystem.Instance.GetTerrainRelayMaintenanceMultiplier(terrain);
            
            total += maintenance * modifier;
        }
        total *= ResearchModifierSystem.Instance.RelayMaintenanceResearchMultiplier;
        return total;
    }
    public float CalculateTrainPurchaseCost(Train train)
    {
        if (train == null || config == null)
            return 0f;
        
        float trainCost = config.BaseTrainPurchaseCost;
        trainCost += (train.SpeedLevel - 1) * config.BaseSpeedUpgradeCost;

        if (ResearchModifierSystem.Instance != null)
            trainCost *= ResearchModifierSystem.Instance.TrainCostResearchMultiplier;
        
        trainCost += train.AttachedTrainConsist.WagonCount * CalculateWagonRefundCost();
        return trainCost;
    }

    public float CalculateTrainRefundCost(Train train) => CalculateTrainPurchaseCost(train) * config.RefundRatio;
    public float CalculateWagonRefundCost()
    {
        if (config == null)
            return 0f;

        float wagonCost = config.BaseWagonUpdateCost;

        if (ResearchModifierSystem.Instance != null)
            wagonCost *= ResearchModifierSystem.Instance.WagonCostResearchMultiplier;

        return wagonCost * config.RefundRatio;
    }
    public float CalculateTrainMaintenance(Train train)
    {
        if (train == null || config == null)
            return 0f;
        TrainConsist consist = train.AttachedTrainConsist;
        float trainMaintenance = consist.GetHeadMaintenance();
        for (int i = 0; i < consist.wagons.Count; i++)
            trainMaintenance += consist.GetUnitMaintenance(i);

        return trainMaintenance;
    }

    public float CalculateTrainRepairCost(Train train)
    {
        if (train == null || config == null)
            return 0f;
        
        float repairCost = config.BaseTrainRepairCost;

        if (ResearchModifierSystem.Instance != null)
            repairCost *= ResearchModifierSystem.Instance.TrainCostResearchMultiplier;
        
        return repairCost;
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

        if (RelayStopRegistry.Instance != null)
            total += CalculateRelayMaintenance();
        
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

    public void HandleTrainCreated(Train train, RailLine line)
    {
        float trainCost = CalculateTrainPurchaseCost(train);
        FinanceSystem.Instance.AdjustBalance(-trainCost);
    }

    public void HandleTrainRemoved(Train train)
    {
        float refund = CalculateTrainRefundCost(train);
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
        RailManager.TopologyChanged += ClearIncomeTokens;
        TrainManager.TrainCreated += HandleTrainCreated;
        TrainManager.TrainRemoved += HandleTrainRemoved;
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        RailManager.LineRemoved -= HandleLineRemoved;
        RailManager.TopologyChanged -= ClearIncomeTokens;
        TrainManager.TrainCreated -= HandleTrainCreated;
        TrainManager.TrainRemoved -= HandleTrainRemoved;
    }

    private float AdvanceIncomeTokens(RailLine line, Vector3Int cell)
    {
        float total = 0f;

        for (int i = trackedIncomeTokens.Count - 1; i >= 0; i--)
        {
            RailIncomeToken token = trackedIncomeTokens[i];

            if (token == null)
            {
                trackedIncomeTokens.RemoveAt(i);
                continue;
            }

            if (token.NextLineIndex < 0 || token.NextLineIndex >= token.RouteLines.Count)
            {
                trackedIncomeTokens.RemoveAt(i);
                continue;
            }

            RailLine nextLine = token.RouteLines[token.NextLineIndex];
            Vector3Int nextCell = token.RouteNodes[token.NextLineIndex + 1];

            if (nextLine != line || nextCell != cell)
                continue;

            token.NextLineIndex++;

            if (token.NextLineIndex >= token.RouteLines.Count)
            {
                total += token.IncomeValue;
                trackedIncomeTokens.RemoveAt(i);
            }
        }

        return total;
    }

    private void TryCreateIncomeTokens(RailLine line, Vector3Int cell)
    {
        if (!TryGetOppositeEndpoint(line, cell, out Vector3Int endpoint))
            return;

        if (!StationRegistry.TryGet(endpoint, out Station endStation))
            return;

        if (StationRegistry.TryGet(cell, out _))
            return;

        if (RelayStopRegistry.Instance == null || !RelayStopRegistry.Instance.IsRelayCell(cell))
            return;

        if (StationEconomySystem.Instance == null)
            return;

        foreach (Station targetStation in StationEconomySystem.Instance.Stations)
        {
            if (targetStation == null || targetStation == endStation)
                continue;

            if (!RailManager.Instance.TryGetShortestPathFull(endStation.Cell, targetStation.Cell, out List<Vector3Int> routeNodes, out List<RailLine> routeLines, out float _))
                continue;

            if (routeLines.Count < 2)
                continue;

            if (routeLines[0] != line)
                continue;

            if (routeNodes.Count < 2 || routeNodes[1] != cell)
                continue;

            if (RouteHasIntermediateStation(routeNodes))
                continue;

            float income = CalculateLineIncome(targetStation, endStation);

            if (income <= 0f)
                continue;

            RailIncomeToken token = new RailIncomeToken
            {
                SourceStation = endStation,
                TargetStation = targetStation,
                RouteNodes = routeNodes,
                RouteLines = routeLines,
                NextLineIndex = 1,
                IncomeValue = income
            };

            trackedIncomeTokens.Add(token);
        }
    }

    private bool IsDirectStationToStationLine(RailLine line) => StationRegistry.TryGet(line.Start, out _) && StationRegistry.TryGet(line.End, out _);

    private bool TryGetOppositeEndpoint(RailLine line, Vector3Int endpoint, out Vector3Int opposite)
    {
        opposite = default;

        if (line == null)
            return false;

        if (line.Start == endpoint)
        {
            opposite = line.End;
            return true;
        }

        if (line.End == endpoint)
        {
            opposite = line.Start;
            return true;
        }

        return false;
    }

    private bool RouteHasIntermediateStation(List<Vector3Int> routeNodes)
    {
        if (routeNodes == null || routeNodes.Count <= 2)
            return false;

        for (int i = 1; i < routeNodes.Count - 1; i++)
        {
            if (StationRegistry.TryGet(routeNodes[i], out _))
                return true;
        }

        return false;
    }

    public void ClearIncomeTokens()
    {
        trackedIncomeTokens.Clear();
    }

}

public class RailIncomeToken
{
    public Station SourceStation;
    public Station TargetStation;
    public List<RailLine> RouteLines;
    public List<Vector3Int> RouteNodes;
    public int NextLineIndex;
    public float IncomeValue;
}
