using System;
using System.Collections.Generic;

[Serializable]
public class RelayStopRegistrySaveData
{
    public int nextID;
    public List<RelayStopSaveData> relays = new();
}