using System;

[Serializable]
public struct DemandRequest
{
    public int StationID;
    public ResourceType Resource;
    public int Amount;

    public DemandRequest(int stationID, ResourceType resource, int amount)
    {
        StationID = stationID;
        Resource = resource;
        Amount = amount;
    }

    public bool IsEmpty => Amount <= 0;
}
