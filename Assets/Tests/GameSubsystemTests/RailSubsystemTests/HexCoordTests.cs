using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HexCoordTesting
{
    [Test]
    public void OffsetAxialConversionTestEvenRow()
    {
        Vector3Int offset = new Vector3Int(3, 2, 0);

        Vector2Int axial = HexCoords.OffsetToAxial(offset);
        Vector3Int result = HexCoords.AxialToOffset(axial);

        Assert.AreEqual(offset, result);
    }

    [Test]
    public void OffsetAxialConversionTestOddRow()
    {
        Vector3Int offset = new Vector3Int(3, 3, 0);

        Vector2Int axial = HexCoords.OffsetToAxial(offset);
        Vector3Int result = HexCoords.AxialToOffset(axial);

        Assert.AreEqual(offset, result);
    }

    [Test]
    public void NeighbourTestEvenRow()
    {
        Vector3Int cell = new Vector3Int(0, 0, 0);

        Assert.AreEqual(new Vector3Int(1, 0, 0), HexCoords.Neighbour(cell, 0));
        Assert.AreEqual(new Vector3Int(0, 1, 0), HexCoords.Neighbour(cell, 1));
        Assert.AreEqual(new Vector3Int(-1, 1, 0), HexCoords.Neighbour(cell, 2));
        Assert.AreEqual(new Vector3Int(-1, 0, 0), HexCoords.Neighbour(cell, 3));
        Assert.AreEqual(new Vector3Int(-1, -1, 0), HexCoords.Neighbour(cell, 4));
        Assert.AreEqual(new Vector3Int(0, -1, 0), HexCoords.Neighbour(cell, 5));
    }

    [Test]
    public void NeighbourTestOddRow()
    {
        Vector3Int cell = new Vector3Int(0, 1, 0);

        Assert.AreEqual(new Vector3Int(1, 1, 0), HexCoords.Neighbour(cell, 0));
        Assert.AreEqual(new Vector3Int(1, 2, 0), HexCoords.Neighbour(cell, 1));
        Assert.AreEqual(new Vector3Int(0, 2, 0), HexCoords.Neighbour(cell, 2));
        Assert.AreEqual(new Vector3Int(-1, 1, 0), HexCoords.Neighbour(cell, 3));
        Assert.AreEqual(new Vector3Int(0, 0, 0), HexCoords.Neighbour(cell, 4));
        Assert.AreEqual(new Vector3Int(1, 0, 0), HexCoords.Neighbour(cell, 5));
    }

    [Test]
    public void AreNeighboursTest()
    {
        Vector3Int cell = new Vector3Int(2, 2, 0);

        for (int dir = 0; dir < 6; dir++)
        {
            Vector3Int neighbor = HexCoords.Neighbour(cell, dir);
            Assert.IsTrue(HexCoords.AreNeighbours(cell, neighbor), $"Direction {dir} faile");
        }
    }

    [Test]
    public void DirIndexTest()
    {
        Vector3Int cell = new Vector3Int(2, 2, 0);

        for (int dir = 0; dir < 6; dir++)
        {
            Vector3Int neighbor = HexCoords.Neighbour(cell, dir);
            Assert.AreEqual(dir, HexCoords.DirIndex(cell, neighbor));
        }
    }

    [Test]
    public void DirIndexTestNotNeighbour()
    {
        Vector3Int a = new Vector3Int(0, 0, 0);
        Vector3Int b = new Vector3Int(5, 5, 0);

        Assert.AreEqual(-1, HexCoords.DirIndex(a, b));
    }
}
