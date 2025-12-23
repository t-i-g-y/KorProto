using System;
using System.Collections.Generic;
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

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

        RailManager.LineCreated += AddRailData;
        RailManager.LineRemoved += RemoveRailData;
    }

    private void OnDestroy()
    {
        RailManager.LineCreated -= AddRailData;
        RailManager.LineRemoved -= RemoveRailData;
    }

    public void AddRailData(RailLine line)
    {
        if (line == null || line.Length < 2) 
            return;

        for (int i = 0; i < line.Length - 1; i++)
        {
            var a = line.Cells[i];
            var b = line.Cells[i + 1];
            int dirAB = HexCoords.DirIndex(a, b);
            int dirBA = HexCoords.DirIndex(b, a);

            if (IsInBounds(a))
            {
                mapRailData[a.x, a.y].RailDirs[dirAB]++;
                UpdateRailDataVisualiser(a.x, a.y);
            }
                
            if (IsInBounds(b))
            {
                mapRailData[b.x, b.y].RailDirs[dirBA]++;
                UpdateRailDataVisualiser(b.x, b.y);
            }
        }
    }

    public void RemoveRailData(RailLine line)
    {
        if (line == null || line.Length < 2) 
            return;

        int dir = -1;

        for (int i = 0; i < line.Length - 1; i++)
        {
            var prevCell = line.End;
            var cell = line.Cells[i];
            var nextCell = line.Cells[i + 1];
            int dirAB = HexCoords.DirIndex(cell, nextCell);
            int dirBA = HexCoords.DirIndex(nextCell, cell);

            if (i == 0)
            {
                dir = HexCoords.DirIndex(cell, nextCell);
                if (IsInBounds(cell))
                {
                    mapRailData[cell.x, cell.y].RailDirs[dirAB]--;
                    if (mapRailData[cell.x, cell.y].RailDirs[dirAB] < 0)
                        Debug.Log($"ERROR: negative value at ({cell.x}, {cell.y}) dir {dirAB}");
                    UpdateRailDataVisualiser(cell.x, cell.y);
                }
            }
            else
            {
                prevCell = line.Cells[i - 1];
                dirAB = HexCoords.DirIndex(cell, nextCell);
                dirBA = HexCoords.DirIndex(cell, prevCell);

                if (IsInBounds(cell))
                {
                    mapRailData[cell.x, cell.y].RailDirs[dirAB]--;
                    if (mapRailData[cell.x, cell.y].RailDirs[dirAB] < 0)
                        Debug.Log($"ERROR: negative value at ({cell.x}, {cell.y}) dir {dirAB}");

                    mapRailData[cell.x, cell.y].RailDirs[dirBA]--;
                    if (mapRailData[cell.x, cell.y].RailDirs[dirBA] < 0)
                        Debug.Log($"ERROR: negative value at ({cell.x}, {cell.y}) dir {dirBA}");
                    UpdateRailDataVisualiser(cell.x, cell.y);
                }
            }
        }

        var lastCell = line.End;
        if (IsInBounds(lastCell))
        {
            dir = HexCoords.DirIndex(lastCell, line.Cells[line.Length - 2]);
            mapRailData[lastCell.x, lastCell.y].RailDirs[dir]--;
            if (mapRailData[lastCell.x, lastCell.y].RailDirs[dir] < 0)
                Debug.Log($"ERROR: negative value at ({lastCell.x}, {lastCell.y}) dir {dir}");
            UpdateRailDataVisualiser(lastCell.x, lastCell.y);
        }
    }

    public int[] GetHexRailDirs(Vector3Int coords) => mapRailData[coords.x, coords.y].RailDirs;

    public TerrainType GetTerrainType(Vector3Int coords) => mapRailData[coords.x, coords.y].HexTerrain;

    private bool AreLinesEqual(RailLine a, RailLine b)
    {
        if (a.Length != b.Length) 
            return false;

        bool forwardMatch = true;
        for (int i = 0; i < a.Length; i++)
        {
            if (a.Cells[i] != b.Cells[i])
            {
                forwardMatch = false;
                break;
            }
        }

        if (forwardMatch) 
            return true;

        for (int i = 0; i < a.Length; i++)
            if (a.Cells[i] != b.Cells[b.Length - 1 - i])
                return false;
        return true;
    }

    private bool AreLinesEqual(RailLine a, List<Vector3Int> ghostCells)
    {
        if (a.Length != ghostCells.Count) 
            return false;

        bool forwardMatch = true;
        for (int i = 0; i < a.Length; i++)
        {
            if (a.Cells[i] != ghostCells[i])
            {
                forwardMatch = false;
                break;
            }
        }
        
        if (forwardMatch) 
            return true;

        for (int i = 0; i < a.Length; i++)
            if (a.Cells[i] != ghostCells[ghostCells.Count - 1 - i])
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

    public bool IsLineDuplicate(List<Vector3Int> ghostPath)
    {
        foreach (var oldLine in RailManager.Instance.Lines)
            if (oldLine.Length == ghostPath.Count && AreLinesEqual(oldLine, ghostPath))
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
    public TerrainType HexTerrain;
    public HexRailData(Vector3Int coords)
    {
        HexCoords = coords;
        RailDirs = new int[6];
        HexTerrain = TerrainType.Grassland;
    }

    public HexRailData(Vector3Int coords, TerrainType terrain)
    {
        HexCoords = coords;
        RailDirs = new int[6];
        HexTerrain = terrain;
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