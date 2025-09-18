using UnityEngine;

public static class HexCoords
{
    private static readonly Vector2Int[] DIRS = {
        new(+1, 0), new(+1,-1), new(0,-1),
        new(-1, 0), new(-1,+1), new(0,+1)
    };

    public static Vector2Int OffsetToAxial(Vector3Int off)
    {
        int q = off.x - ((off.y - (off.y & 1)) / 2);
        int r = off.y;
        return new Vector2Int(q, r);
    }

    public static Vector3Int AxialToOffset(Vector2Int ax)
    {
        int x = ax.x + ((ax.y - (ax.y & 1)) / 2);
        int y = ax.y;
        return new Vector3Int(x, y, 0);
    }

    public static bool AreNeighbors(Vector3Int aOff, Vector3Int bOff)
    {
        var a = OffsetToAxial(aOff);
        for (int i = 0; i < 6; i++)
        {
            if (AxialToOffset(a + DIRS[i]) == bOff)
                return true;
        }
        return false;
    }
}
