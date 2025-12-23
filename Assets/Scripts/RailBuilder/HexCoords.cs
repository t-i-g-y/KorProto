using UnityEngine;

public static class HexCoords
{
    private static readonly Vector2Int[] DIRS = {
        new(1, 0),
        new(1 ,-1),
        new(0, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1)
    };

    private static readonly Vector3Int[] OffEven = {
        new(+1,  0, 0), // E
        new( 0, +1, 0), // NE
        new(-1, +1, 0), // NW
        new(-1,  0, 0), // W
        new(-1, -1, 0), // SW
        new(0, -1, 0), // SE
    };
    private static readonly Vector3Int[] OffOdd = {
        new(+1,  0, 0), // E
        new(+1, +1, 0), // NE
        new( 0, +1, 0), // NW
        new(-1,  0, 0), // W
        new( 0, -1, 0), // SW
        new(+1, -1, 0), // SE
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
    
    public static Vector3Int Neighbor(Vector3Int a, int dir)
    {
        var offs = ((a.y & 1) != 0) ? OffOdd : OffEven;
        return a + offs[dir];
    }
        

    public static int DirIndex(Vector3Int a, Vector3Int b)
    {
        var offs = ((a.y & 1) != 0) ? OffOdd : OffEven;
        var d = b - a;
        for (int i = 0; i < 6; i++)
            if (offs[i] == d)
                return i;
        Debug.Log($"Impossible dir={d}");
        return -1;
    }

    public static int[] GetDoubleSidedDirs(Vector3Int cellA, Vector3Int cellB)
    {
        int dAB = HexCoords.DirIndex(cellA, cellB);
        int dBA = HexCoords.DirIndex(cellB, cellA);

        return new int[] {dAB, dBA};
    }
}
