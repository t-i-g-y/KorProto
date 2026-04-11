using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RelayStopSaveData
{
    public int ID;
    public Vector3Int cell;
    public float maintenancePerDay;
    public List<ResourceAmountSaveData> storedCargo = new();
    public List<ResourceDestinationQueueSaveData> cargoDestinations = new();
}