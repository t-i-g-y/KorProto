using System.Collections.Generic;
using UnityEngine;

public class StationEconomySystem : MonoBehaviour
{
    public static StationEconomySystem Instance { get; private set; }
    [SerializeField] private List<Station> connectedStations = new();
    public List<Station> Stations => connectedStations;

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

    private void OnEnable()
    {
        RailManager.ActiveNetworkChanged += RefreshStationData;
        RailManager.LineCreated += OnTopologyChanged;
        RailManager.LineRemoved += OnTopologyChanged;
    }

    private void OnDisable()
    {
        RailManager.ActiveNetworkChanged -= RefreshStationData;
        RailManager.LineCreated -= OnTopologyChanged;
        RailManager.LineRemoved -= OnTopologyChanged;
    }

    private void OnTopologyChanged(RailLine _)
    {
        RefreshStationData();
    }

    public void ApplyStationEconomyTick()
    {
        RefreshStationData();
    }
    public void RefreshStationData()
    {
        connectedStations.Clear();

        if (RailManager.Instance == null)
            return;

        HashSet<Station> unique = new();

        foreach (var line in RailManager.Instance.ActiveLines)
        {
            if (line == null)
                continue;

            AddStationIfActive(line.Start, unique);
            AddStationIfActive(line.End, unique);
        }

        connectedStations.AddRange(unique);
        GlobalDemandSystem.Instance?.SyncDemandRequests(connectedStations);
    }

    private void AddStationIfActive(Vector3Int cell, HashSet<Station> unique)
    {
        if (!RailManager.Instance.IsCellInActiveNetwork(cell))
            return;

        if (!StationRegistry.TryGet(cell, out Station station))
            return;

        if (station != null)
            unique.Add(station);
    }
}
