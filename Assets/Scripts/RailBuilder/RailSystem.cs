using System;
using UnityEngine;


public class RailSystem : MonoBehaviour
{
    public static RailSystem Instance { get; private set; }
    [SerializeField] private int minX, minY;
    [SerializeField] private int maxX, maxY;
    private HexRailData[,] mapRailData;
    [SerializeField] private HexRailDataRow[] mapRailDataVisualiser;

    void Awake()
    {   
        int width = maxX - minX;
        int height = maxY - minY;

        mapRailData = new HexRailData[width, height];
        mapRailDataVisualiser = new HexRailDataRow[height];
        for (int h = 0; h < height; h++)
            mapRailDataVisualiser[h] = new HexRailDataRow(width);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                mapRailData[i, j] = new HexRailData(new Vector3Int(i, j, 0));
                UpdateRailDataVisualiser(i, j);
            }
        }

        Instance = this;
    }

    public void AddRailData(RailLine line)
    {
        if (line == null || line.Length < 2) 
            return;

        for (int i = 0; i < line.Length - 1; i++)
        {
            var a = line.Cells[i];
            var b = line.Cells[i + 1];
            int dAB = HexCoords.DirIndex(a, b);
            int dBA = HexCoords.DirIndex(b, a);

            if (IsInBounds(a))
            {
                mapRailData[a.x, a.y].RailDirs[dAB]++;
                UpdateRailDataVisualiser(a.x, a.y);
            }
                
            if (IsInBounds(b))
            {
                mapRailData[b.x, b.y].RailDirs[dBA]++;
                UpdateRailDataVisualiser(b.x, b.y);
            }
        }
    }

    public void RemoveRailData(RailLine line)
    {
        if (line == null || line.Length < 2) 
            return;

        for (int i = 0; i < line.Length - 1; i++)
        {
            var a = line.Cells[i];
            var b = line.Cells[i + 1];
            int dAB = HexCoords.DirIndex(a, b);
            int dBA = HexCoords.DirIndex(b, a);

            if (IsInBounds(a))
            {
                mapRailData[a.x, a.y].RailDirs[dAB]--;
                if (mapRailData[a.x, a.y].RailDirs[dAB] < 0)
                    Debug.Log($"ERROR: negative value at ({a.x}, {a.y}) dir {dAB}");
                UpdateRailDataVisualiser(a.x, a.y);
            }

            if (IsInBounds(b))
            {
                mapRailData[b.x, b.y].RailDirs[dBA]--;
                if (mapRailData[b.x, b.y].RailDirs[dBA] < 0)
                    Debug.Log($"ERROR: negative value at ({b.x}, {b.y}) dir {dBA}");
                UpdateRailDataVisualiser(b.x, b.y);
            }
        }
    }

    public int[] GetHexRailDirs(Vector3Int coords) => mapRailData[coords.x, coords.y].RailDirs;

    private bool AreLinesEqual(RailLine a, RailLine b)
    {
        for (int i = 0; i < a.Length; i++)
            if (a.Cells[i] != b.Cells[i])
                return false;
        
        return true;
    }

    public bool IsLineDuplicate(RailLine newLine)
    {
        foreach (var oldLine in RailManager.Instance.Lines)
            if (oldLine.Length == newLine.Length && AreLinesEqual(oldLine, newLine))
                return true;
        
        return false;
    }
    private bool IsInBounds(Vector3Int coords) => coords.x >= 0 && coords.x < mapRailData.GetLength(0) && coords.y >= 0 && coords.y < mapRailData.GetLength(1);

    private void UpdateRailDataVisualiser(int r, int q)
    {
        mapRailDataVisualiser[r].row[q] = mapRailData[r, q];
    }
    
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

[Serializable]
public struct HexRailDataRow
{
    public HexRailData[] row;
    public HexRailDataRow(int width)
    {
        row = new HexRailData[width];
    }
}