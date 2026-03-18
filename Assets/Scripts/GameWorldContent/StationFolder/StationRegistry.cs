using System.Collections.Generic;
using UnityEngine;

public static class StationRegistry
{
    private static int nextID = 0;
    private static readonly Dictionary<Vector3Int, Station> stationByCell = new();
    private static readonly Dictionary<int, Station> stationByID = new();
    public static void Register(Station station)
    {
        station.StationID = nextID++;
        stationByCell[station.Cell] = station;
        stationByID[station.StationID] = station; 
    }
    public static void Unregister(Station station)
    {
        if (stationByCell.TryGetValue(station.Cell, out var cur) && cur == station)
        {
            stationByCell.Remove(station.Cell);
            stationByID.Remove(station.StationID);
        }
            
    }
    public static bool TryGet(Vector3Int cell, out Station station) => stationByCell.TryGetValue(cell, out station);
    public static bool TryGet(int ID, out Station station) => stationByID.TryGetValue(ID, out station);
}
