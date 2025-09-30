using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailPainter : MonoBehaviour
{
    [SerializeField] private Tilemap railA;
    [SerializeField] private Tilemap railB;
    [SerializeField] private RailVisualSet visuals;

    public void ClearCell(Vector3Int c)
    {
        railA.SetTile(c, null);
        railB.SetTile(c, null);
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
}

