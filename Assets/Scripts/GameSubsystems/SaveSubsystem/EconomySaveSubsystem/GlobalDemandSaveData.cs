using System;
using System.Collections.Generic;

[Serializable]
public class GlobalDemandSystemSaveData
{
    public List<StationTransitSaveData> stationTransit = new();
}

[Serializable]
public class StationTransitSaveData
{
    public int stationID;
    public int resourceType;
    public int amount;
    public List<int> destinationStationIDs = new();
}
