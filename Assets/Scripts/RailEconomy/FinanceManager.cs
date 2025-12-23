using System;
using UnityEngine;

public class FinanceManager : MonoBehaviour
{
    private float balance;
    private float lastBalanceChange;
    private float dayBalance;
    private int currentDay;
    [SerializeField] private TerrainConfig terrainConfig;
    [SerializeField] private ResourcePriceConfig resourcePriceConfig;
    
    public float Balance => balance;
    public float LastBalanceChange => lastBalanceChange;
    public float DayBalance
    {
        get => dayBalance;
        /*
        set
        {
            if (currentDay != TimeManager.Instance.DayCounter)
            {
                currentDay = TimeManager.Instance.DayCounter;
                dayBalance = 0f;
            }
            Debug.Log($"Daily balance change: {value}");
            dayBalance += value;
        }*/
        set => dayBalance = value;
    }
    public int CurrentDay
    {
        get => currentDay;
        set => currentDay = value;
    }
    public static FinanceManager Instance { get; private set; }

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
        currentDay = 0;
        balance = 100f;
        lastBalanceChange = 0f;
        dayBalance = 0f;
        RailManager.LineCreated += DeductForLineConstruction;
        RailManager.LineRemoved += RefundForLineRemoval;
    }

    private void OnDestroy()
    {
        RailManager.LineCreated -= DeductForLineConstruction;
        RailManager.LineRemoved -= RefundForLineRemoval;
    }

    public void AdjustBalance(float amount)
    {
        lastBalanceChange = amount;
        balance += amount;
        Debug.Log($"Balance: {balance}");
        DayBalance += amount;
        Debug.Log($"Current balance of the day: {dayBalance}");
    }

    public void DeductForLineConstruction(RailLine line)
    {
        float cost = CalculateLineConstructionCost(line);
        AdjustBalance(-cost);
    }

    private float GetTerrainCost(Vector3Int cell)
    {
        TerrainType hexTerrain = RailSystem.Instance.GetTerrainType(cell);
        return terrainConfig.TerrainBaseCost * terrainConfig.TerrainCostMultipliers[(int)hexTerrain].Multiplier;
    }

    private float CalculateLineConstructionCost(RailLine line)
    {
        float cost = 0;
        foreach (var cell in line.Cells)
        {
            float terrainCost = GetTerrainCost(cell);
            cost += terrainCost;
        }
        return cost;
    }

    public void RefundForLineRemoval(RailLine line)
    {
        float refund = CalculateLineRefund(line);
        AdjustBalance(refund);
    }

    private float CalculateLineRefund(RailLine line)
    {
        float cost = CalculateLineConstructionCost(line);
        return cost * 0.5f;
    }

    public void GenerateIncomeForRailLine(RailLine line)
    {
        float income = CalculateIncome(line);
        AdjustBalance(income);
    }

    private float CalculateIncome(RailLine line)
    {
        int stationCount = 0;
        if (StationRegistry.TryGet(line.Start, out var startStation))
            stationCount++;
        if (StationRegistry.TryGet(line.End, out var endStation)) 
            stationCount++;

        if (stationCount == 2)
            return 100f;
        else if (stationCount == 1)
            return 50f;
        else
            return 0f;
    }

    public void DeductMaintenanceCost(float maintenanceCost)
    {
        AdjustBalance(-maintenanceCost);
    }

    public void SellResource(ResourceType resource) => AdjustBalance(resourcePriceConfig.ResourcePriceList[(int)resource].Price);
}

