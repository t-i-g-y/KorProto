using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrainConsistSaveData
{
    public TrainConsistUnitSaveData headLocomotive;
    public List<TrainConsistUnitSaveData> wagons = new();
    public List<ResourceAmountSaveData> cargo = new();
    public List<ResourceDestinationQueueSaveData> cargoDestinations = new();
}
