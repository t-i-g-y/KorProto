using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailPainter : MonoBehaviour
{
    [Header("Committed layers")]
    [SerializeField] private Tilemap[] rails;
    [SerializeField] private RailVisualSet visuals;

    [Header("Ghost layers")]
    [SerializeField] private Tilemap ghost;
    [SerializeField] private RailVisualSet ghostVisuals;

    [Header("Scene")]
    [SerializeField] private RailSystem system;

    public void ClearGhost()
    {
        ghost?.ClearAllTiles();
    }

    public void PaintRails(RailLine railLine, bool isSelected)
    {
        StringBuilder sb = new StringBuilder($"Dir map of Line {railLine.ID}:");
        var cells = railLine.Cells;
        int dirAB = -1;
        var startTiles = isSelected ? visuals.SelectedStartTiles : visuals.StartTiles;
        var endTiles = isSelected ? visuals.SelectedEndTiles : visuals.EndTiles;
        for (int i = 0; i < railLine.Length - 1; i++)
        {
            if (i == 0)
            {
                dirAB = HexCoords.DirIndex(cells[i], cells[i + 1]);
                if (dirAB > 5 || dirAB < 0)
                {
                    Debug.Log($"Impossible dir={dirAB}");
                    return;
                }
                rails[dirAB].SetTile(cells[i], startTiles[dirAB]);
                sb.Append($" {dirAB} ->");
            }
            else
            {
                var dirs = HexCoords.GetDoubleSidedDirs(cells[i], cells[i + 1]);
                dirAB = HexCoords.DirIndex(cells[i], cells[i + 1]);
                int dirBA = HexCoords.DirIndex(cells[i], cells[i - 1]);
                if (dirAB > 5 || dirAB < 0)
                {
                    Debug.Log($"Impossible dirAB={dirAB}");
                    return;
                }
                else if (dirBA > 5 || dirBA < 0)
                {
                    Debug.Log($"Impossible dirBA={dirBA}");
                    return;
                }
                rails[dirAB].SetTile(cells[i], startTiles[dirAB]);
                rails[dirBA].SetTile(cells[i], startTiles[dirBA]);
                sb.Append($" {dirBA}-{dirAB} ");
            }
            
        }
        rails[dirAB].SetTile(railLine.End, endTiles[dirAB]);
        sb.Append($"-> {dirAB}");
        Debug.Log(sb.ToString());
    }

    public void UnpaintRails(RailLine railLine)
    {
        StringBuilder sb = new StringBuilder($"Dir map of Line for removal {railLine.ID}:");
        var cells = railLine.Cells;
        int dir = -1;
        for (int i = 0; i < railLine.Length - 1; i++)
        {
            dir = HexCoords.DirIndex(cells[i], cells[i + 1]);
            if (system.GetHexRailDirs(cells[i])[dir] == 0)
                rails[dir].SetTile(cells[i], null);
            sb.Append($" {dir}");
        }
        if (system.GetHexRailDirs(railLine.End)[dir] == 0)
            rails[dir].SetTile(railLine.End, null);
        sb.Append($" {dir}.");
        Debug.Log(sb.ToString());
    }

    public void PaintGhostPath(List<Vector3Int> path)
    {
        ClearGhost();
        if (path == null || path.Count < 2 || ghost == null)
            return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            int dir = HexCoords.DirIndex(path[i], path[i + 1]);
            if (dir > 5 || dir < 0)
                return;
            ghost.SetTile(path[i], ghostVisuals.StartTiles[dir]);
        }

        int lastDir = HexCoords.DirIndex(path[^2], path[^1]);
        if (lastDir >= 0 && lastDir <= 5)
            ghost.SetTile(path[^1], ghostVisuals.EndTiles[lastDir]);
    }
}

