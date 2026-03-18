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
    private Queue<int>[] cargoDestinations;
    [SerializeField] private bool debugCargo;
    public int WagonCount => wagons.Count;
    public ResourceAmount[] Cargo => cargo;

    private void Awake()
    {
        if (headLocomotive == null)
            headLocomotive = new TrainConsistUnit(defaultHeadCapacity, defaultHeadMaintenance);

        EnsureCargoInitialized();
        EnsureDestinationQueuesInitialized();
        RecalculateCapacity();
    }
    

    public bool TryLoadOne(Station station, bool onlyLoadRequested)
    {
        if (station == null)
            return false;

        if (usedCapacity >= totalCapacity)
            return false;

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            if (!station.Produces(resource))
                continue;

            if (onlyLoadRequested && !GlobalDemandSystem.Instance.HasOutstandingDemand(resource))
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

    public bool TryLoadOneFromStation(Station station, ResourceType resource)
    {
        if (station == null || usedCapacity >= totalCapacity)
            return false;

        if (!station.Produces(resource))
            return false;

        if (station.GetSupplyAmount(resource) <= 0)
            return false;

        if (!station.TryTakeSupply(resource, 1))
            return false;

        cargo[(int)resource].Amount++;
        usedCapacity++;
        return true;
    }

    public bool TryLoadOneFromStation(Station station, ResourceType resource, int destinationStationID)
    {
        if (station == null || usedCapacity >= totalCapacity)
            return false;

        EnsureDestinationQueuesInitialized();

        if (!station.Produces(resource))
            return false;

        if (station.GetSupplyAmount(resource) <= 0)
            return false;

        if (!station.TryTakeSupply(resource, 1))
            return false;

        cargo[(int)resource].Amount++;
        cargoDestinations[(int)resource].Enqueue(destinationStationID);
        usedCapacity++;

        if (debugCargo)
                Debug.Log($"[TrainConsist] loaded resource={resource} for stationID={destinationStationID}");
                
        return true;
    }

    public bool TryLoadOneFromRelay(RelayStop relay, ResourceType resource)
    {
        if (relay == null || usedCapacity >= totalCapacity)
            return false;

        int taken = relay.Take(resource, 1);
        if (taken <= 0)
            return false;

        cargo[(int)resource].Amount += taken;
        usedCapacity += taken;
        return true;
    }

    public bool TryLoadOneFromRelay(RelayStop relay, ResourceType resource, int destinationStationID)
    {
        if (relay == null || usedCapacity >= totalCapacity)
            return false;

        EnsureDestinationQueuesInitialized();

        if (!relay.PeekNextDestination(resource, out int peekedDestination))
            return false;

        if (peekedDestination != destinationStationID)
            return false;

        if (!relay.TakeOne(resource, out int actualDestination))
            return false;

        cargo[(int)resource].Amount++;
        cargoDestinations[(int)resource].Enqueue(actualDestination);
        usedCapacity++;

        if (debugCargo)
                Debug.Log($"[TrainConsist] loaded resource={resource} for stationID={destinationStationID} at relayID={relay.ID}");

        return true;
    }

    public bool TryLoadOneFromStationTransit(Station station, ResourceType resource, int destinationStationId)
    {
        if (station == null || usedCapacity >= totalCapacity)
            return false;

        EnsureDestinationQueuesInitialized();

        if (!GlobalDemandSystem.Instance.PeekStationTransitDestination(station.StationID, resource, out int peekedDestination))
            return false;

        if (peekedDestination != destinationStationId)
            return false;

        if (!GlobalDemandSystem.Instance.TakeStationTransitOne(station.StationID, resource, out int actualDestination))
            return false;

        cargo[(int)resource].Amount++;
        cargoDestinations[(int)resource].Enqueue(actualDestination);
        usedCapacity++;

        if (debugCargo)
            Debug.Log($"[TrainConsist] load resource={resource} at stationID={station.StationID} for destination={actualDestination}");

        return true;
    }

    public CargoSaleResult TryUnloadOne(Station station)
    {
        if (station == null)
            return CargoSaleResult.None;

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

            float soldValue = FinanceSystem.Instance != null ? FinanceSystem.Instance.SellResource(resource) : 0f;

            station.TrySatisfyDemand(resource, 1);
            GlobalDemandSystem.Instance?.FulfillDemand(station.StationID, resource, 1);

            return new CargoSaleResult(true, resource, soldValue);
        }

        return CargoSaleResult.None;
    }

    public CargoSaleResult TryUnloadOneToStation(Station station)
    {
        if (station == null)
            return CargoSaleResult.None;

        EnsureDestinationQueuesInitialized();

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            int resourceIndex = (int)resource;
            if (cargo[resourceIndex].Amount <= 0)
                continue;

            if (cargoDestinations[resourceIndex].Count <= 0)
                continue;

            int destinationStationID = cargoDestinations[resourceIndex].Peek();
            if (destinationStationID != station.StationID)
                continue;

            if (!station.Consumes(resource))
                continue;

            if (station.GetDemandAmount(resource) <= 0)
                continue;

            cargoDestinations[resourceIndex].Dequeue();
            cargo[resourceIndex].Amount--;
            usedCapacity--;

            float soldValue = FinanceSystem.Instance != null ? FinanceSystem.Instance.SellResource(resource) : 0f;

            station.TrySatisfyDemand(resource, 1);
            GlobalDemandSystem.Instance?.FulfillDemand(station.StationID, resource, 1);

            if (debugCargo)
                Debug.Log($"[TrainConsist] unloaded resource={resource} for stationID={destinationStationID}");

            return new CargoSaleResult(true, resource, soldValue);
        }

        return CargoSaleResult.None;
    }

    public bool TryUnloadOneToRelay(RelayStop relay)
    {
        if (relay == null)
            return false;

        EnsureDestinationQueuesInitialized();

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            int index = (int)resource;
            if (cargo[index].Amount <= 0)
                continue;

            if (cargoDestinations[index].Count <= 0)
                continue;

            int destinationStationID = cargoDestinations[index].Dequeue();
            cargo[index].Amount--;
            usedCapacity--;

            relay.Add(resource, 1, destinationStationID);
            if (debugCargo)
                Debug.Log($"[TrainConsist] unloaded resource={resource} for stationID={destinationStationID} at relayID={relay.ID}");
            return true;
        }

        return false;
    }

    public bool TryUnloadOneToStationTransit(Station station)
    {
        if (station == null)
            return false;

        EnsureDestinationQueuesInitialized();

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            int index = (int)resource;
            if (cargo[index].Amount <= 0)
                continue;

            if (cargoDestinations[index].Count <= 0)
                continue;

            int destinationStationID = cargoDestinations[index].Peek();

            if (destinationStationID == station.StationID)
                continue;

            cargoDestinations[index].Dequeue();
            cargo[index].Amount--;
            usedCapacity--;

            GlobalDemandSystem.Instance?.AddStationTransit(station.StationID, resource, 1, destinationStationID);

            if (debugCargo)
                Debug.Log($"[TrainConsist] stored resource{resource} at stationID={station.StationID} for destination={destinationStationID}");

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

        if (cargoDestinations == null || cargoDestinations.Length != resourceTypes.Length)
        {
            cargoDestinations = new Queue<int>[resourceTypes.Length];
            for (int i = 0; i < resourceTypes.Length; i++)
                cargoDestinations[i] = new Queue<int>();
        }
    }

    private void EnsureDestinationQueuesInitialized()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        if (cargoDestinations == null || cargoDestinations.Length != resourceTypes.Length)
        {
            cargoDestinations = new Queue<int>[resourceTypes.Length];
            for (int i = 0; i < resourceTypes.Length; i++)
                cargoDestinations[i] = new Queue<int>();
        }

        for (int i = 0; i < resourceTypes.Length; i++)
        {
            cargoDestinations[i] ??= new Queue<int>();
        }
    }
}
