using UnityEngine;

public class HexCoords
{
    public static Vector2Int OffsetToAxial(Vector3Int off) {
        int q = off.x - ((off.y - (off.y & 1)) / 2);
        int r = off.y;
        return new Vector2Int(q, r);
    }

    public static Vector3Int AxialToOffset(Vector2Int ax) {
        int x = ax.x + ((ax.y - (ax.y & 1)) / 2);
        int y = ax.y;
        return new Vector3Int(x, y, 0);
    }

    private static readonly Vector2Int[] DIRS = new[]{
        new Vector2Int(+1, 0), new Vector2Int(+1,-1), new Vector2Int(0,-1),
        new Vector2Int(-1, 0), new Vector2Int(-1,+1), new Vector2Int(0,+1)
    };

    public static Vector2Int AxialNeighbor(Vector2Int a, int dir) => a + DIRS[dir];
}
