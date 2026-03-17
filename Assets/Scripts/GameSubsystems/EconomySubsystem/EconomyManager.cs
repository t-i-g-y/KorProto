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

    private void Update()
    {
        if (TimeManager.Instance == null)
            return;

        int currentDay = TimeManager.Instance.DayCounter;
        if (currentDay == lastProcessedDay)
            return;

        lastProcessedDay = currentDay;
        TickEconomy();
    }

    public void TickEconomy()
    {
        if (stationEconomySystem != null)
            stationEconomySystem.RefreshStationData();

        if (globalDemandSystem != null && stationEconomySystem != null)
            globalDemandSystem.SyncDemandRequests(stationEconomySystem.Stations);

        if (railEconomySystem != null)
            railEconomySystem.ApplyRailEconomyTick();

        if (financeSystem != null)
            financeSystem.CurrentDay = lastProcessedDay;
    }

    public void ForceResync()
    {
        if (stationEconomySystem == null || globalDemandSystem == null)
            return;

        stationEconomySystem.RefreshStationData();
        globalDemandSystem.SyncDemandRequests(stationEconomySystem.Stations);
    }
}
