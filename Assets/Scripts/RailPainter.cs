using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailPainter : MonoBehaviour
{
    [SerializeField] Tilemap railA;   // layer for End
    [SerializeField] Tilemap railB;   // layer for Start
    [SerializeField] RailVisualSet visuals;

    public void ClearCell(Vector3Int c)
    {
        railA.SetTile(c, null);
        railB.SetTile(c, null);
    }

    public void PaintRails(RailLine railLine)
    {
        StringBuilder sb = new StringBuilder($"Dir map of Line {railLine.ID}:");
        var cells = railLine.Cells;
        int dir = -1;
        for (int i = 0; i < railLine.Length - 1; i++)
        {
            dir = HexCoords.DirIndex(cells[i], cells[i + 1]);
            if (dir > 5 || dir < 0)
            {
                Debug.Log($"Impossible dir={dir}");
                return;
            }
            railA.SetTile(cells[i], visuals.StartTiles[dir]);
            sb.Append($" {dir}");
        }
        railA.SetTile(railLine.End, visuals.EndTiles[dir]);
        sb.Append($" {dir}.");
        Debug.Log(sb.ToString());
        
        
        








        /*





        var path = railLine.Cells;


        
        // START cell
        int d0 = HexCoords.DirIndex(path[0], path[1]); // toward next
        ClearCell(path[0]);
        if (d0 >= 0 && visuals.StartTiles[d0])
            railB.SetTile(path[0], visuals.StartTiles[d0]);

        // INTERIOR cells
        for (int i = 1; i < path.Count-1; i++){
            int dIn  = HexCoords.DirIndex(path[i], path[i-1]); // from prev
            int dOut = HexCoords.DirIndex(path[i], path[i+1]); // to next
            ClearCell(path[i]);
            if (dIn  >= 0 && visuals.EndTiles[dIn])
                railA.SetTile(path[i], visuals.EndTiles[dIn]);
            if (dOut >= 0 && visuals.StartTiles[dOut])
                railB.SetTile(path[i], visuals.StartTiles[dOut]);
        }

        // END cell
        int z = path.Count - 1;
        int dz = HexCoords.DirIndex(path[z], path[z-1]); // from prev
        ClearCell(path[z]);
        if (dz >= 0 && visuals.EndTiles[dz]) railA.SetTile(path[z], visuals.EndTiles[dz]);
        */
    }
}

