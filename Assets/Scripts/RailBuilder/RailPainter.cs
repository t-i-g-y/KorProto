using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailPainter : MonoBehaviour
{
    [Header("Committed layers")]
    [SerializeField] private Tilemap railA;
    [SerializeField] private Tilemap railB;
    [SerializeField] private RailVisualSet visuals;

    [Header("Ghost layers")]
    [SerializeField] private Tilemap ghostA;
    [SerializeField] private Tilemap ghostB;
    [SerializeField] private RailVisualSet ghostVisuals;

    public void ClearCell(Vector3Int c)
    {
        railA?.SetTile(c, null);
        railB?.SetTile(c, null);
    }

    public void ClearGhost()
    {
        ghostA?.ClearAllTiles();
        ghostB?.ClearAllTiles();
    }

    public void PaintRails(RailLine railLine, bool isSelected)
    {
        StringBuilder sb = new StringBuilder($"Dir map of Line {railLine.ID}:");
        var cells = railLine.Cells;
        int dir = -1;
        var startTiles = isSelected ? visuals.SelectedStartTiles : visuals.StartTiles;
        var endTiles = isSelected ? visuals.SelectedEndTiles : visuals.EndTiles;
        for (int i = 0; i < railLine.Length - 1; i++)
        {
            dir = HexCoords.DirIndex(cells[i], cells[i + 1]);
            if (dir > 5 || dir < 0)
            {
                Debug.Log($"Impossible dir={dir}");
                return;
            }
            railA.SetTile(cells[i], startTiles[dir]);
            sb.Append($" {dir}");
        }
        railA.SetTile(railLine.End, endTiles[dir]);
        sb.Append($" {dir}.");
        Debug.Log(sb.ToString());
    }

    public void PaintGhostPath(List<Vector3Int> path)
    {
        ClearGhost();
        if (path == null || path.Count < 2 || ghostA == null || ghostB == null)
            return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            int dir = HexCoords.DirIndex(path[i], path[i + 1]);
            if (dir > 5 || dir < 0)
                return;
            ghostB.SetTile(path[i], ghostVisuals.StartTiles[dir]);
        }

        int lastDir = HexCoords.DirIndex(path[^1], path[^2]);
        if (lastDir >= 0 && lastDir <= 5)
            ghostA.SetTile(path[^1], ghostVisuals.EndTiles[lastDir]);
    }
}

