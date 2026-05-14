using UnityEngine;
using System;

public class FinanceSystem : MonoBehaviour
{
    private float balance;
    private float lastBalanceChange;
    private float dayBalance;
    private int currentDay;
    private int daysBelowGameOverThreshold;
    private bool gameOverTriggered;

    [SerializeField] private EconomyConfig economyConfig;
    [SerializeField] private float gameOverBalanceThreshold = -1000f;
    [SerializeField] private int maxDaysBelowThreshold = 5;
    [SerializeField] private GameObject gameOverPanel;

    public float Balance => balance;
    public float LastBalanceChange => lastBalanceChange;
    public int DaysLeftBeforeGameOver => Mathf.Max(0, maxDaysBelowThreshold - daysBelowGameOverThreshold);
    public bool IsInGameOverWarning => balance <= gameOverBalanceThreshold && !gameOverTriggered;
    public event Action<int> OnGameOverWarningChanged;

    public float DayBalance
    {
        get => dayBalance;
        set => dayBalance = value;
    }

    public int CurrentDay
    {
        get => currentDay;
        set => currentDay = value;
    }

    public static FinanceSystem Instance { get; private set; }

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

        Initialize();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void Initialize()
    {
        currentDay = 0;
        balance = 100f;
        lastBalanceChange = 0f;
        dayBalance = 0f;
    }

    private void ShowGameOver()
    {
        MenuPauseState.SetPaused(true);

        if (TimeManager.Instance != null)
            TimeManager.Instance.Pause();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    private void UpdateGameOverState()
    {
        if (gameOverTriggered)
            return;

        if (balance <= gameOverBalanceThreshold)
            daysBelowGameOverThreshold++;
        else
            daysBelowGameOverThreshold = 0;

        OnGameOverWarningChanged?.Invoke(DaysLeftBeforeGameOver);

        if (daysBelowGameOverThreshold >= maxDaysBelowThreshold)
        {
            gameOverTriggered = true;
            ShowGameOver();
        }
    }

    public void ApplyFinanceSystemTick()
    {
        currentDay++;
        dayBalance = 0;
        UpdateGameOverState();
    }

    public void AdjustBalance(float amount)
    {
        lastBalanceChange = amount;
        balance += amount;
        DayBalance += amount;

        Debug.Log($"Balance: {balance}");
        Debug.Log($"Current balance of the day: {dayBalance}");

        if (balance > gameOverBalanceThreshold && daysBelowGameOverThreshold > 0)
        {
            daysBelowGameOverThreshold = 0;
            OnGameOverWarningChanged?.Invoke(DaysLeftBeforeGameOver);
        }
    }

    public void PayConstruction(float amount)
    {
        if (amount > 0f)
            AdjustBalance(-amount);
    }

    public void ApplyRefund(float amount)
    {
        if (amount > 0f)
            AdjustBalance(amount);
    }

    public void DeductMaintenanceCost(float maintenanceCost)
    {
        if (maintenanceCost > 0f)
            AdjustBalance(-maintenanceCost);
    }

    public float SellResource(ResourceType resource)
    {
        float value = GetCargoValue(resource);
        AdjustBalance(value);
        return value;
    }

    public float GetCargoValue(ResourceType resource)
    {
        if (economyConfig == null)
            return 0f;

        float value = economyConfig.GetCargoValue(resource);

        if (ResearchModifierSystem.Instance != null)
            value *= ResearchModifierSystem.Instance.CargoSaleIncomeResearchMultiplier;

        return value;
    }
    
    #region save subsystem
    public FinanceSystemSaveData GetSaveData()
    {
        return new FinanceSystemSaveData
        {
            balance = balance,
            lastBalanceChange = lastBalanceChange,
            dayBalance = dayBalance,
            currentDay = currentDay,
            daysBelowGameOverThreshold = daysBelowGameOverThreshold,
            gameOverTriggered = gameOverTriggered
        };
    }

    public void LoadFromSaveData(FinanceSystemSaveData data)
    {
        if (data == null)
            return;

        balance = data.balance;
        lastBalanceChange = data.lastBalanceChange;
        dayBalance = data.dayBalance;
        currentDay = data.currentDay;
        daysBelowGameOverThreshold = data.daysBelowGameOverThreshold;
        gameOverTriggered = data.gameOverTriggered;
    }
    #endregion
}

