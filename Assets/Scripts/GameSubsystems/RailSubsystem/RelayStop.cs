using System;
using UnityEngine;

public class RelayStop : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private Vector3Int cell;
    [SerializeField] private float maintenancePerDay = 1f;
    [SerializeField] private ResourceAmount[] storedCargo;

    public int ID => id;
    public Vector3Int Cell => cell;
    public float MaintenancePerDay => maintenancePerDay;
    public ResourceAmount[] StoredCargo => storedCargo;

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
    }
}
