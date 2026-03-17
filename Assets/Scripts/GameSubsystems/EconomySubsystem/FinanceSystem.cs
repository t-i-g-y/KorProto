using UnityEngine;

public class FinanceSystem : MonoBehaviour
{
    private float balance;
    private float lastBalanceChange;
    private float dayBalance;
    private int currentDay;

    [SerializeField] private EconomyConfig economyConfig;

    public float Balance => balance;
    public float LastBalanceChange => lastBalanceChange;

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

        currentDay = 0;
        balance = 100f;
        lastBalanceChange = 0f;
        dayBalance = 0f;
    }

    public void AdjustBalance(float amount)
    {
        lastBalanceChange = amount;
        balance += amount;
        DayBalance += amount;

        Debug.Log($"Balance: {balance}");
        Debug.Log($"Current balance of the day: {dayBalance}");
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
        if (economyConfig == null)
            return 0f;

        float value = economyConfig.GetCargoValue(resource);
        AdjustBalance(value);
        return value;
    }
    
}

