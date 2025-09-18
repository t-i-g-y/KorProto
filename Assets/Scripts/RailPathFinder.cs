using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RailPathFinder : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap Land;
    public Tilemap Water;

    bool IsLand(Vector3Int hex) => Land.HasTile(hex) && !Water.HasTile(hex);

    public List<Vector3Int> FindShortestPath(Vector3Int startOff, Vector3Int endOff)
    {
        var start = HexCoords.OffsetToAxial(startOff);
        var goal  = HexCoords.OffsetToAxial(endOff);

        var q = new Queue<Vector2Int>();
        var came = new Dictionary<Vector2Int, Vector2Int>();
        var seen = new HashSet<Vector2Int>();

        q.Enqueue(start);
        seen.Add(start);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == goal)
                break;
            /*
            for (int d = 0; d < 6; d++)
            {
                var nbAx = HexCoords.AxialNeighbor(cur, d);
                var nbOff = HexCoords.AxialToOffset(nbAx);
                if (!IsLand(nbOff))
                    continue;
                if (seen.Contains(nbAx))
                    continue;

                seen.Add(nbAx);
                came[nbAx] = cur;
                q.Enqueue(nbAx);
            }
            */
        }

        if (!came.ContainsKey(goal) && start != goal)
            return new List<Vector3Int>();

        var pathAx = new List<Vector2Int> { goal };
        var curAx  = goal;
        while (curAx != start)
        {
            curAx = came[curAx];
            pathAx.Add(curAx);
        }
        pathAx.Reverse();

        var path = new List<Vector3Int>(pathAx.Count);
        foreach (var a in pathAx) path.Add(HexCoords.AxialToOffset(a));
        return path;
    }
}
