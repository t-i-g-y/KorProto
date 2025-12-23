using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailSystem : MonoBehaviour
{
    public static RailSystem Instance { get; private set; }
    [SerializeField] private int minX, minY;
    [SerializeField] private int maxX, maxY;
    private HexRailData[,] mapRailData;
    [SerializeField] private Tilemap land;
    [SerializeField] private Tilemap water;
    [SerializeField] private Sprite[] terrainTileSprites;
    [SerializeField] private TerrainType[] terrainTypes;
    private Dictionary<Sprite, TerrainType> spriteToTerrain;
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
        if (minX < 0)
            width++;
        if (minY < 0)
            height++;

        mapRailData = new HexRailData[width, height];
        mapRailDataVisualiser = new HexRailDataRow[height];
        for (int y = 0; y < height; y++)
            mapRailDataVisualiser[y] = new HexRailDataRow(width);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var worldCell = new Vector3Int(x + minX, y + minY, 0);
                mapRailData[x, y] = new HexRailData(worldCell);
                UpdateRailDataVisualiser(x, y);
            }
        }

        RailManager.LineCreated += AddRailData;
        RailManager.LineRemoved += RemoveRailData;

        
    }

    private void Start()
    {
        spriteToTerrain = new Dictionary<Sprite, TerrainType>();
        for (int i = 0; i < Mathf.Min(terrainTileSprites.Length, terrainTypes.Length); i++)
            if (terrainTileSprites[i] != null && !spriteToTerrain.ContainsKey(terrainTileSprites[i]))
                spriteToTerrain.Add(terrainTileSprites[i], terrainTypes[i]);
        
        for (int x = 0; x < mapRailData.GetLength(0); x++)
        {
            for (int y = 0; y < mapRailData.GetLength(1); y++)
            {
                var worldCell = new Vector3Int(x + minX, y + minY, 0);

                bool waterPresent = false;
                if (water != null)
                {
                    if (water.GetTile(worldCell) != null) 
                        waterPresent = true;
                    else if (water.GetInstantiatedObject(worldCell) != null) 
                        waterPresent = true;
                }

                TerrainType terrain = GetTerrainTypeFromSprite(worldCell, waterPresent);

                var data = mapRailData[x, y];
                data.HexTerrain = terrain;
                mapRailData[x, y] = data;

                UpdateRailDataVisualiser(x, y);
            }
        }
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
                mapRailData[a.x - minX, a.y - minY].RailDirs[dirAB]++;
                UpdateRailDataVisualiser(a.x - minX, a.y - minY);
            }
                
            if (IsInBounds(b))
            {
                mapRailData[b.x - minX, b.y - minY].RailDirs[dirBA]++;
                UpdateRailDataVisualiser(b.x - minX, b.y - minY);
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
                    mapRailData[cell.x - minX, cell.y - minY].RailDirs[dirAB]--;
                    if (mapRailData[cell.x - minX, cell.y - minY].RailDirs[dirAB] < 0)
                        Debug.Log($"ERROR: negative value at ({cell.x}, {cell.y}) dir {dirAB}");
                    UpdateRailDataVisualiser(cell.x - minX, cell.y - minY);
                }
            }
            else
            {
                prevCell = line.Cells[i - 1];
                dirAB = HexCoords.DirIndex(cell, nextCell);
                dirBA = HexCoords.DirIndex(cell, prevCell);

                if (IsInBounds(cell))
                {
                    mapRailData[cell.x - minX, cell.y - minY].RailDirs[dirAB]--;
                    if (mapRailData[cell.x - minX, cell.y - minY].RailDirs[dirAB] < 0)
                        Debug.Log($"ERROR: negative value at ({cell.x}, {cell.y}) dir {dirAB}");

                    mapRailData[cell.x - minX, cell.y - minY].RailDirs[dirBA]--;
                    if (mapRailData[cell.x - minX, cell.y - minY].RailDirs[dirBA] < 0)
                        Debug.Log($"ERROR: negative value at ({cell.x}, {cell.y}) dir {dirBA}");
                    UpdateRailDataVisualiser(cell.x - minX, cell.y - minY);
                }
            }
        }

        var lastCell = line.End;
        if (IsInBounds(lastCell))
        {
            dir = HexCoords.DirIndex(lastCell, line.Cells[line.Length - 2]);
            mapRailData[lastCell.x - minX, lastCell.y - minY].RailDirs[dir]--;
            if (mapRailData[lastCell.x - minX, lastCell.y - minY].RailDirs[dir] < 0)
                Debug.Log($"ERROR: negative value at ({lastCell.x}, {lastCell.y}) dir {dir}");
            UpdateRailDataVisualiser(lastCell.x - minX, lastCell.y - minY);
        }
    }

    public int[] GetHexRailDirs(Vector3Int cell) => mapRailData[cell.x - minX, cell.y - minY].RailDirs;

    public TerrainType GetTerrainType(Vector3Int cell) => mapRailData[cell.x - minX, cell.y - minY].HexTerrain;
    public TerrainType GetTerrainTypeFromSprite(Vector3Int cell, bool isWater = false)
    {
        if (isWater)
        {
            var waterTileBase = water.GetTile(cell);
            if (waterTileBase == null) 
                return TerrainType.Lake;

            if (waterTileBase is Tile waterTile && waterTile.sprite != null)
            {
                if (spriteToTerrain.TryGetValue(waterTile.sprite, out var terrain)) 
                    return terrain;
            }

            var waterObj = land.GetInstantiatedObject(cell);
            if (waterObj != null)
            {
                var sr = waterObj.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null && spriteToTerrain.TryGetValue(sr.sprite, out var terrain)) 
                    return terrain;
            }

            return TerrainType.Lake;
        }

        var landTileBase = land.GetTile(cell);
        if (landTileBase == null) 
            return TerrainType.Grassland;

        if (landTileBase is Tile tile && tile.sprite != null)
        {
            if (spriteToTerrain.TryGetValue(tile.sprite, out var terrain)) 
                return terrain;
        }

        var landObj = land.GetInstantiatedObject(cell);
        if (landObj != null)
        {
            var sr = landObj.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null && spriteToTerrain.TryGetValue(sr.sprite, out var terrain)) 
                return terrain;
        }

        return TerrainType.Grassland;
    }
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
    private bool IsInBounds(Vector3Int cell) => (cell.x - minX) >= 0 && (cell.x - minX) < mapRailData.GetLength(0) && (cell.y - minY) >= 0 && (cell.y - minY) < mapRailData.GetLength(1);

    private void UpdateRailDataVisualiser(int x, int y)
    {
        if (mapRailDataVisualiser == null) 
            return;
        if (y >= mapRailDataVisualiser.Length) 
            return;
        var row = mapRailDataVisualiser[y];
        if (row.row == null) 
            return;
        if (x >= row.row.Length) 
            return;
        row.row[x] = mapRailData[x, y];
        mapRailDataVisualiser[y] = row;
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