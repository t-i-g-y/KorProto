using System;
using UnityEngine;

public class RailSystem : MonoBehaviour
{
    public static RailSystem Instance { get; private set; }
    [SerializeField] private int minX, minY;
    [SerializeField] private int maxX, maxY;
    [SerializeField] private HexRailData[,] mapRailData;

    void Awake()
    {   
        int width = maxX - minX;
        int height = maxY - minY;

        mapRailData = new HexRailData[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                mapRailData[i, j] = new HexRailData(new Vector3Int(i, j, 0));
            }
        }

        Instance = this;
    }

    public void AddRailData(RailLine line)
    {
        int dir = -1;
        for (int i = 0; i < line.Length - 1; i++)
        {
            dir = HexCoords.DirIndex(line.Cells[i], line.Cells[i + 1]);
            mapRailData[line.Cells[i].x, line.Cells[i].y].RailDirs[dir]++;
        }
        mapRailData[line.Cells[^1].x, line.Cells[^1].y].RailDirs[dir]++;
    }

    public void RemoveRailData(RailLine line)
    {
        int dir = -1;
        for (int i = 0; i < line.Length - 1; i++)
        {
            dir = HexCoords.DirIndex(line.Cells[i], line.Cells[i + 1]);
            mapRailData[line.Cells[i].x, line.Cells[i].y].RailDirs[dir]--;
            if (mapRailData[line.Cells[i].x, line.Cells[i].y].RailDirs[dir] < 0)
                Debug.Log($"ERROR: negative value at ({line.Cells[i].x}, {line.Cells[i].y})");
        }
        mapRailData[line.Cells[^1].x, line.Cells[^1].y].RailDirs[dir]--;
    }

    public int[] GetHexRailDirs(Vector3Int coords) => mapRailData[coords.x, coords.y].RailDirs;
}

[Serializable]
public struct HexRailData
{
    public Vector3Int HexCoords;
    public int[] RailDirs;

    public HexRailData(Vector3Int coords)
    {
        HexCoords = coords;
        RailDirs = new int[6];
    }
}