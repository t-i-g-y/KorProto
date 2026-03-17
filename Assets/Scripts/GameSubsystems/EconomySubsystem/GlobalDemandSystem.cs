using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDemandSystem : MonoBehaviour
{
    public static GlobalDemandSystem Instance { get; private set; }

    private Dictionary<ResourceType, List<DemandRequest>> demandRequests = new();
    [SerializeField] private ResourceAmount[] outstandingTotals;
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

    public void SyncDemandRequests(List<Station> stations)
    {
        ClearAll();

        if (stations == null)
            return;

        foreach (Station station in stations)
        {
            if (station == null)
                continue;

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                int amount = station.GetDemandAmount(resource);
                if (amount <= 0)
                    continue;

                outstandingTotals[(int)resource].Amount += amount;
                demandRequests[resource].Add(
                    new DemandRequest(station.StationID, resource, amount)
                );
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
            outstandingTotals[(int)resource].Amount =
                Mathf.Max(0, outstandingTotals[(int)resource].Amount - amount);

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

    private void ClearAll()
    {
        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            outstandingTotals[(int)resource].Amount = 0;
            demandRequests[resource].Clear();
        }
    }
}