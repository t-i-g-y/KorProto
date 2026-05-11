using TMPro;
using UnityEngine;

public class ResearchIncomeSystem : MonoBehaviour
{
    public static ResearchIncomeSystem Instance { get; private set; }
    [SerializeField] private int globalResearchPerHour = 1;
    [SerializeField] private TMP_Text researchIncomeText;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnHourChanged += HandleHourChanged;
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnHourChanged -= HandleHourChanged;
    }

    private void HandleHourChanged(int day, int hour)
    {
        TickResearchIncome();
    }

    private void TickResearchIncome()
    {
        if (ResearchSystem.Instance == null)
            return;

        int globalIncome = CalculateGlobalIncomePerHour();
        int localIncome = CalculateLocalIncomePerHour();
        int total = globalIncome + localIncome;
        Debug.Log($"added {total} research");
        SetResearchPointDisplayText(total);
        ResearchSystem.Instance.AddResearchPoints(total);
    }

    private int CalculateGlobalIncomePerHour()
    {
        float value = globalResearchPerHour;

        if (ResearchModifierSystem.Instance != null)
            value *= ResearchModifierSystem.Instance.GlobalResearchIncomeMultiplier;

        return Mathf.RoundToInt(value);
    }

    private int CalculateLocalIncomePerHour()
    {
        if (StationEconomySystem.Instance == null)
            return 0;

        int total = 0;

        foreach (Station station in StationEconomySystem.Instance.Stations)
            total += GetStationResearchIncome(station);

        float value = total;

        if (ResearchModifierSystem.Instance != null)
            value *= ResearchModifierSystem.Instance.LocalResearchIncomeMultiplier;

        return Mathf.RoundToInt(value);
    }

    private int GetStationResearchIncome(Station station)
    {
        if (station == null)
            return 0;

        int total = 0;

        foreach (StationAttribute attribute in station.Attributes)
        {
            switch (attribute.AttributeType)
            {
                case StationAttributeType.Port:
                    total += 2;
                    break;
                default:
                    total += 1;
                    break;
            }
        }

        return total;
    }

    public int AddGlobalResearchPerHour(int rp) => globalResearchPerHour += rp;
    private void SetResearchPointDisplayText(int income)
    {
        string text = $"+{income}";
        researchIncomeText.text = text;
    }
}