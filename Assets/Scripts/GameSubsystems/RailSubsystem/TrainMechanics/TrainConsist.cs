using System;
using System.Collections.Generic;
using UnityEngine;

public class TrainConsist : MonoBehaviour
{
    public TrainConsistUnit headLocomotive;
    public List<TrainConsistUnit> wagons = new();
    public int usedCapacity;
    public int totalCapacity;
    public float loadSpeed = 0.5f;
    public float unloadSpeed = 0.5f;
    [SerializeField] private int defaultHeadCapacity = 6;
    [SerializeField] private int defaultWagonCapacity = 6;
    [SerializeField] private float defaultHeadMaintenance = 1f;
    [SerializeField] private float defaultWagonMaintenance = 1f;
    [SerializeField] private ResourceAmount[] cargo;

    public int WagonCount => wagons.Count;
    public ResourceAmount[] Cargo => cargo;

    private void Awake()
    {
        if (headLocomotive == null)
            headLocomotive = new TrainConsistUnit(defaultHeadCapacity, defaultHeadMaintenance);

        EnsureCargoInitialized();
        RecalculateCapacity();
    }
    
    public bool TryUnloadOne(Station station)
    {
        if (station == null)
            return false;

        foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (!station.Consumes(resource))
                continue;

            int resourceIndex = (int)resource;
            if (cargo[resourceIndex].Amount <= 0)
                continue;

            if (station.GetDemandAmount(resource) <= 0)
                continue;

            cargo[resourceIndex].Amount--;
            usedCapacity--;
            station.TrySatisfyDemand(resource, 1);
            FinanceManager.Instance.SellResource(resource);
            return true;
        }

        return false;
    }

    public bool TryLoadOne(Station station, bool onlyLoadRequested)
    {
        if (station == null)
            return false;

        if (usedCapacity >= totalCapacity)
            return false;

        foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType)))
        {
            if (!station.Produces(resource))
                continue;

            if (onlyLoadRequested && GlobalDemand.Outstanding[(int)resource].Amount <= 0)
                continue;

            if (station.GetSupplyAmount(resource) <= 0)
                continue;

            if (!station.TryTakeSupply(resource, 1))
                continue;

            cargo[(int)resource].Amount++;
            usedCapacity++;
            return true;
        }

        return false;
    }

    public void ChangeHeadCapacity(int delta)
    {
        if (headLocomotive == null)
            return;

        headLocomotive.capacity = Mathf.Max(0, headLocomotive.capacity + delta);
        RecalculateCapacity();
    }

    public void ChangeHeadMaintenance(float multiplier)
    {
        if (headLocomotive == null)
            return;

        headLocomotive.maintenance = Mathf.Max(0f, headLocomotive.maintenance * multiplier);
    }

    public bool TryAddWagon()
    {
        wagons.Add(new TrainConsistUnit(defaultWagonCapacity, defaultWagonMaintenance));
        RecalculateCapacity();
        return true;
    }

    public void RemoveLastWagon()
    {
        if (wagons.Count == 0)
            return;

        wagons.RemoveAt(wagons.Count - 1);
        ClampCargoToCapacity();
        RecalculateCapacity();
    }

    public ResourceAmount[] BuildCargoSlice(int startIndex, int capacity)
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        ResourceAmount[] slice = new ResourceAmount[resourceTypes.Length];

        for (int i = 0; i < resourceTypes.Length; i++)
            slice[i] = new ResourceAmount(resourceTypes[i]);

        int remainingToSkip = Mathf.Max(0, startIndex);
        int remainingToTake = Mathf.Max(0, capacity);

        foreach (ResourceType resourceType in resourceTypes)
        {
            int cargoAmount = cargo[(int)resourceType].Amount;

            if (remainingToSkip >= cargoAmount)
            {
                remainingToSkip -= cargoAmount;
                continue;
            }

            int visibleAmount = Mathf.Min(cargoAmount - remainingToSkip, remainingToTake);
            slice[(int)resourceType].Amount = visibleAmount;
            remainingToTake -= visibleAmount;
            remainingToSkip = 0;

            if (remainingToTake <= 0)
                break;
        }

        return slice;
    }

    public int GetHeadCapacity()
    {
        return headLocomotive != null ? headLocomotive.capacity : 0;
    }

    public int GetUnitCapacity(int wagonIndex)
    {
        if (wagonIndex < 0 || wagonIndex >= wagons.Count)
            return 0;

        return wagons[wagonIndex].capacity;
    }

    private void RecalculateCapacity()
    {
        totalCapacity = 0;

        if (headLocomotive != null)
            totalCapacity += Mathf.Max(0, headLocomotive.capacity);

        foreach (TrainConsistUnit wagon in wagons)
        {
            if (wagon != null)
                totalCapacity += Mathf.Max(0, wagon.capacity);
        }

        usedCapacity = CountCargo();
    }

    private void ClampCargoToCapacity()
    {
        int allowed = Mathf.Max(0, GetPlannedCapacityWithoutRecount());
        int current = CountCargo();

        if (current <= allowed)
            return;

        int overflow = current - allowed;

        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        for (int i = resourceTypes.Length - 1; i >= 0 && overflow > 0; i--)
        {
            int removable = Mathf.Min(cargo[i].Amount, overflow);
            cargo[i].Amount -= removable;
            overflow -= removable;
        }

        usedCapacity = CountCargo();
    }

    private int GetPlannedCapacityWithoutRecount()
    {
        int result = 0;

        if (headLocomotive != null)
            result += Mathf.Max(0, headLocomotive.capacity);

        foreach (TrainConsistUnit wagon in wagons)
        {
            if (wagon != null)
                result += Mathf.Max(0, wagon.capacity);
        }

        return result;
    }

    private int CountCargo()
    {
        int total = 0;

        if (cargo == null)
            return 0;

        foreach (ResourceAmount amount in cargo)
            total += Mathf.Max(0, amount.Amount);

        return total;
    }

    private void EnsureCargoInitialized()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        if (cargo == null || cargo.Length != resourceTypes.Length)
            cargo = new ResourceAmount[resourceTypes.Length];

        for (int i = 0; i < resourceTypes.Length; i++)
        {
            if (cargo[i].Type != resourceTypes[i])
                cargo[i] = new ResourceAmount(resourceTypes[i], Mathf.Max(0, cargo[i].Amount));
        }
    }
}
