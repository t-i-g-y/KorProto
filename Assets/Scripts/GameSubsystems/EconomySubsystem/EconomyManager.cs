using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private EconomyConfig config;
    [SerializeField] private FinanceSystem financeSystem;
    [SerializeField] private StationEconomySystem stationEconomySystem;
    [SerializeField] private GlobalDemandSystem globalDemandSystem;
    [SerializeField] private RailEconomySystem railEconomySystem;

    private int lastProcessedDay = -1;

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

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged += HandleDayChanged;
            TimeManager.Instance.OnHourChanged += HandleHourChanged;
        }
            
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayChanged -= HandleDayChanged;
            TimeManager.Instance.OnHourChanged -= HandleHourChanged;
        }
            
    }

    private void HandleHourChanged(int day, int newHour)
    {
        if (globalDemandSystem != null && stationEconomySystem != null)
            globalDemandSystem.ApplyGlobalDemandSystemTick(stationEconomySystem.Stations);
    }

    private void HandleDayChanged(int newDay)
    {
        if (newDay == lastProcessedDay)
            return;
        lastProcessedDay = newDay;
        TickEconomy();
    }
    public void TickEconomy()
    {
        if (financeSystem != null)
            financeSystem.ApplyFinanceSystemTick();

        if (stationEconomySystem != null)
            stationEconomySystem.ApplyStationEconomyTick();

        if (globalDemandSystem != null && stationEconomySystem != null)
            globalDemandSystem.ApplyGlobalDemandSystemTick(stationEconomySystem.Stations);

        if (railEconomySystem != null)
            railEconomySystem.ApplyRailEconomyTick();

    }

    public void ForceResync()
    {
        if (stationEconomySystem == null || globalDemandSystem == null)
            return;

        stationEconomySystem.RefreshStationData();
        globalDemandSystem.SyncDemandRequests(stationEconomySystem.Stations);
    }


    #region save subsystem
    public EconomyManagerSaveData GetSaveData()
    {
        return new EconomyManagerSaveData
        {
            lastProcessedDay = lastProcessedDay
        };
    }

    public void LoadFromSaveData(EconomyManagerSaveData data)
    {
        if (data == null)
            return;

        lastProcessedDay = data.lastProcessedDay;
        ForceResync();
    }
    #endregion
}
