using System.Collections.Generic;
using UnityEngine;

public static class StationRegistry
{
    private static int nextID = 0;
    private static readonly Dictionary<Vector3Int, Station> stationByCell = new();
    public static void Register(Station station)
    {
        station.StationID = nextID++;
        stationByCell[station.Cell] = station;
    }
    public static void Unregister(Station station)
    {
        if (stationByCell.TryGetValue(station.Cell, out var cur) && cur == station)
            stationByCell.Remove(station.Cell);
    }
    public static bool TryGet(Vector3Int cell, out Station station) => stationByCell.TryGetValue(cell, out station);
}
