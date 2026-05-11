using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceDestinationQueueSaveData
{
    public int resourceType;
    public List<int> destinationStationIDs = new();
}