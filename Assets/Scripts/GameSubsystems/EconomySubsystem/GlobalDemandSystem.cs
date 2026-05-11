using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDemandSystem : MonoBehaviour
{
    public static GlobalDemandSystem Instance { get; private set; }

    private Dictionary<ResourceType, List<DemandRequest>> demandRequests = new();
    private Dictionary<int, int>[] stationTransitAmounts;
    private Dictionary<int, Queue<int>>[] stationTransitDestinations;
    [SerializeField] private ResourceAmount[] outstandingTotals;
    [SerializeField] private bool debugRouting;
    [SerializeField] private bool debugTransit;

    public Dictionary<ResourceType, List<DemandRequest>> DemandRequests => demandRequests;
    public ResourceAmount[] OutstandingTotals => outstandingTotals;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStorage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeStorage()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        outstandingTotals = new ResourceAmount[resourceTypes.Length];

        foreach (ResourceType resource in resourceTypes)
        {
            outstandingTotals[(int)resource] = new ResourceAmount(resource, 0);
            demandRequests[resource] = new List<DemandRequest>();
        }
    }

    public void ApplyGlobalDemandSystemTick(List<Station> stations)
    {
        SyncDemandRequests(stations);
    }

    public void SyncDemandRequests(List<Station> stations)
    {
        ClearAll();

        if (stations == null || RailManager.Instance == null)
            return;

        foreach (Station station in stations)
        {
            if (station == null)
                continue;

            if (!RailManager.Instance.IsCellInActiveNetwork(station.Cell))
                continue;

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                int amount = station.GetDemandAmount(resource);
                if (amount <= 0)
                    continue;

                outstandingTotals[(int)resource].Amount += amount;
                demandRequests[resource].Add(new DemandRequest(station.StationID, resource, amount));
            }
        }
    }

    public bool HasOutstandingDemand(ResourceType resource)
    {
        return outstandingTotals != null &&
               outstandingTotals[(int)resource].Amount > 0;
    }

    public int GetOutstanding(ResourceType resource)
    {
        return outstandingTotals == null ? 0 : outstandingTotals[(int)resource].Amount;
    }

    public List<DemandRequest> GetRequests(ResourceType resource)
    {
        return demandRequests.TryGetValue(resource, out var list) ? list : new List<DemandRequest>();
    }

    public void FulfillDemand(int stationID, ResourceType resource, int amount)
    {
        if (amount <= 0)
            return;

        if (outstandingTotals != null)
            outstandingTotals[(int)resource].Amount = Mathf.Max(0, outstandingTotals[(int)resource].Amount - amount);

        if (!demandRequests.TryGetValue(resource, out var list))
            return;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].StationID != stationID)
                continue;

            DemandRequest request = list[i];
            int fulfilled = Mathf.Min(amount, request.Amount);
            request.Amount -= fulfilled;
            amount -= fulfilled;

            if (request.Amount <= 0)
                list.RemoveAt(i--);
            else
                list[i] = request;

            if (amount <= 0)
                break;
        }
    }

    public bool TryGetBestDestinationForResource(Vector3Int currentCell, Vector3Int twinnedCell, ResourceType resource, out int destinationStationID)
    {
        destinationStationID = -1;

        if (RailManager.Instance == null)
            return false;

        if (!demandRequests.TryGetValue(resource, out var requests) || requests.Count == 0)
            return false;

        bool found = false;
        float bestCost = float.MaxValue;
        float bestValue = float.MinValue;

        foreach (DemandRequest request in requests)
        {
            if (request.Amount <= 0)
                continue;

            if (!StationRegistry.TryGet(request.StationID, out Station destination) || destination == null)
                continue;

            if (!RailManager.Instance.TryGetShortestPathFirstHop(currentCell, destination.Cell, out Vector3Int firstHop, out float routeCost))
                continue;

            if (firstHop != twinnedCell)
                continue;

            float unitValue = FinanceSystem.Instance != null ? FinanceSystem.Instance.GetCargoValue(resource) : 0f;

            if (!found || routeCost < bestCost || (Mathf.Approximately(routeCost, bestCost) && unitValue > bestValue))
            {
                found = true;
                bestCost = routeCost;
                bestValue = unitValue;
                destinationStationID = request.StationID;
            }

            if (debugRouting)
                Debug.Log($"[Demandrequest] resource={resource} current={currentCell} twinned={twinnedCell} candidateStation={request.StationID}");
        }
        
        return found;
    }
    private void ClearAll()
    {
        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            outstandingTotals[(int)resource].Amount = 0;
            demandRequests[resource].Clear();
        }
    }

    private void EnsureTransitInitialized()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        if (stationTransitAmounts == null || stationTransitAmounts.Length != resourceTypes.Length)
        {
            stationTransitAmounts = new Dictionary<int, int>[resourceTypes.Length];
            stationTransitDestinations = new Dictionary<int, Queue<int>>[resourceTypes.Length];

            for (int i = 0; i < resourceTypes.Length; i++)
            {
                stationTransitAmounts[i] = new Dictionary<int, int>();
                stationTransitDestinations[i] = new Dictionary<int, Queue<int>>();
            }
        }
    }

    public void AddStationTransit(int stationID, ResourceType resource, int amount, int destinationStationID)
    {
        if (amount <= 0)
            return;

        EnsureTransitInitialized();

        int index = (int)resource;

        if (!stationTransitAmounts[index].ContainsKey(stationID))
            stationTransitAmounts[index][stationID] = 0;

        if (!stationTransitDestinations[index].TryGetValue(stationID, out Queue<int> queue))
        {
            queue = new Queue<int>();
            stationTransitDestinations[index][stationID] = queue;
        }

        stationTransitAmounts[index][stationID] += amount;
        for (int i = 0; i < amount; i++)
            queue.Enqueue(destinationStationID);

        if (debugTransit)
            Debug.Log($"[TransitStation] stored resource={resource} at stationID={stationID} for destinationstationID={destinationStationID}");
    }

    public int GetStationTransitAmount(int stationID, ResourceType resource)
    {
        EnsureTransitInitialized();

        int index = (int)resource;
        return stationTransitAmounts[index].TryGetValue(stationID, out int amount) ? amount : 0;
    }

    public bool PeekStationTransitDestination(int stationID, ResourceType resource, out int destinationStationID)
    {
        EnsureTransitInitialized();

        int index = (int)resource;
        if (stationTransitDestinations[index].TryGetValue(stationID, out Queue<int> queue) && queue.Count > 0)
        {
            destinationStationID = queue.Peek();
            return true;
        }

        destinationStationID = -1;
        return false;
    }

    public bool TakeStationTransitOne(int stationID, ResourceType resource, out int destinationStationID)
    {
        EnsureTransitInitialized();

        int index = (int)resource;

        if (!stationTransitAmounts[index].TryGetValue(stationID, out int amount) || amount <= 0)
        {
            destinationStationID = -1;
            return false;
        }

        if (!stationTransitDestinations[index].TryGetValue(stationID, out Queue<int> queue) || queue.Count <= 0)
        {
            destinationStationID = -1;
            return false;
        }

        stationTransitAmounts[index][stationID] = amount - 1;
        destinationStationID = queue.Dequeue();

        if (debugTransit)
            Debug.Log($"[TransitStation] gave resource={resource} at stationID={stationID} for destinationstationID={destinationStationID}");

        return true;
    }

    #region 
    public GlobalDemandSystemSaveData GetSaveData()
    {
        EnsureTransitInitialized();

        var data = new GlobalDemandSystemSaveData();

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            int index = (int)resource;

            foreach (var stationTransitAmount in stationTransitAmounts[index])
            {
                int stationID = stationTransitAmount.Key;
                int amount = stationTransitAmount.Value;

                var entry = new StationTransitSaveData
                {
                    stationID = stationID,
                    resourceType = index,
                    amount = amount
                };

                if (stationTransitDestinations[index].TryGetValue(stationID, out Queue<int> queue) && queue != null)
                    entry.destinationStationIDs.AddRange(queue);

                data.stationTransit.Add(entry);
            }
        }

        return data;
    }

    public void LoadFromSaveData(GlobalDemandSystemSaveData data)
    {
        EnsureTransitInitialized();

        for (int i = 0; i < stationTransitAmounts.Length; i++)
        {
            stationTransitAmounts[i].Clear();
            stationTransitDestinations[i].Clear();
        }

        if (data == null || data.stationTransit == null)
            return;

        foreach (var entry in data.stationTransit)
        {
            int index = entry.resourceType;
            if (index < 0 || index >= stationTransitAmounts.Length)
                continue;

            stationTransitAmounts[index][entry.stationID] = entry.amount;
            stationTransitDestinations[index][entry.stationID] = new Queue<int>(entry.destinationStationIDs ?? new List<int>());
        }
    }
    #endregion
}