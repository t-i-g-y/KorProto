using System;
using System.Collections.Generic;
using UnityEngine;

public class RelayStop : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private Vector3Int cell;
    [SerializeField] private float maintenancePerDay = 1f;
    [SerializeField] private ResourceAmount[] storedCargo;
    [SerializeField] private bool debugRelay;

    public int ID => id;
    public Vector3Int Cell => cell;
    public float MaintenancePerDay => maintenancePerDay;
    public ResourceAmount[] StoredCargo => storedCargo;
    private Queue<int>[] cargoDestinations;
    public void Initialize(int newId, Vector3Int newCell, float maintenance)
    {
        id = newId;
        cell = newCell;
        maintenancePerDay = maintenance;
        EnsureStorage();
    }

    public int GetAmount(ResourceType resource)
    {
        EnsureStorage();
        return storedCargo[(int)resource].Amount;
    }

    public void Add(ResourceType resource, int amount)
    {
        if (amount <= 0)
            return;

        EnsureStorage();
        storedCargo[(int)resource].Amount += amount;
    }

    public int Take(ResourceType resource, int amount)
    {
        if (amount <= 0)
            return 0;

        EnsureStorage();
        int available = storedCargo[(int)resource].Amount;
        int taken = Mathf.Min(available, amount);
        storedCargo[(int)resource].Amount -= taken;
        return taken;
    }

    public void Add(ResourceType resource, int amount, int destinationStationID)
    {
        if (amount <= 0)
            return;

        EnsureStorage();

        storedCargo[(int)resource].Amount += amount;
        for (int i = 0; i < amount; i++)
            cargoDestinations[(int)resource].Enqueue(destinationStationID);
        
        if (debugRelay)
            Debug.Log($"[Relay]RelayID={ID} stored {resource} for stationID={destinationStationID}");
    }

    public bool PeekNextDestination(ResourceType resource, out int destinationStationID)
    {
        EnsureStorage();

        if (cargoDestinations[(int)resource].Count > 0)
        {
            destinationStationID = cargoDestinations[(int)resource].Peek();
            return true;
        }

        destinationStationID = -1;
        return false;
    }

    public bool TakeOne(ResourceType resource, out int destinationStationID)
    {
        EnsureStorage();

        int index = (int)resource;
        if (storedCargo[index].Amount <= 0 || cargoDestinations[index].Count <= 0)
        {
            destinationStationID = -1;
            return false;
        }

        storedCargo[index].Amount--;
        destinationStationID = cargoDestinations[index].Dequeue();

        if (debugRelay)
            Debug.Log($"[Relay]RelayID={ID} gave {resource} for stationID={destinationStationID}");
    
        return true;
        
    }

    private void EnsureStorage()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        if (storedCargo == null || storedCargo.Length != resourceTypes.Length)
            storedCargo = new ResourceAmount[resourceTypes.Length];

        for (int i = 0; i < resourceTypes.Length; i++)
        {
            if (storedCargo[i].Type != resourceTypes[i])
                storedCargo[i] = new ResourceAmount(resourceTypes[i], Mathf.Max(0, storedCargo[i].Amount));
        }

        if (cargoDestinations == null || cargoDestinations.Length != resourceTypes.Length)
        {
            cargoDestinations = new Queue<int>[resourceTypes.Length];
            for (int i = 0; i < resourceTypes.Length; i++)
                cargoDestinations[i] = new Queue<int>();
        }

        for (int i = 0; i < resourceTypes.Length; i++)
        {
            cargoDestinations[i] ??= new Queue<int>();

            if (storedCargo[i].Type != resourceTypes[i])
                storedCargo[i] = new ResourceAmount(resourceTypes[i], Mathf.Max(0, storedCargo[i].Amount));
        }
    }

    #region save subsystem
    public RelayStopSaveData GetSaveData()
    {
        EnsureStorage();

        var data = new RelayStopSaveData
        {
            ID = id,
            cell = cell,
            maintenancePerDay = maintenancePerDay
        };

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            int index = (int)resource;

            data.storedCargo.Add(new ResourceAmountSaveData
            {
                resourceType = index,
                amount = storedCargo != null && index < storedCargo.Length ? storedCargo[index].Amount : 0
            });

            var queueData = new ResourceDestinationQueueSaveData { resourceType = index };

            if (cargoDestinations != null && index < cargoDestinations.Length && cargoDestinations[index] != null)
                queueData.destinationStationIDs.AddRange(cargoDestinations[index]);

            data.cargoDestinations.Add(queueData);
        }

        return data;
    }

    public void LoadFromSaveData(RelayStopSaveData data)
    {
        if (data == null)
            return;

        Initialize(data.ID, data.cell, data.maintenancePerDay);

        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        storedCargo = new ResourceAmount[resourceTypes.Length];
        for (int i = 0; i < resourceTypes.Length; i++)
            storedCargo[i] = new ResourceAmount(resourceTypes[i], 0);

        if (data.storedCargo != null)
        {
            foreach (var cargoData in data.storedCargo)
            {
                if (cargoData.resourceType < 0 || cargoData.resourceType >= storedCargo.Length)
                    continue;

                storedCargo[cargoData.resourceType] = new ResourceAmount((ResourceType)cargoData.resourceType, cargoData.amount);
            }
        }

        cargoDestinations = new Queue<int>[resourceTypes.Length];
        for (int i = 0; i < resourceTypes.Length; i++)
            cargoDestinations[i] = new Queue<int>();

        if (data.cargoDestinations != null)
        {
            foreach (var queueData in data.cargoDestinations)
            {
                if (queueData.resourceType < 0 || queueData.resourceType >= cargoDestinations.Length)
                    continue;

                cargoDestinations[queueData.resourceType] = new Queue<int>(queueData.destinationStationIDs ?? new List<int>());
            }
        }
    }
    #endregion
}
