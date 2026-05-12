using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RailLineTests
{
    private static List<Vector3Int> TestCells()
    {
        return new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0)
        };
    }

    [Test]
    public void RailLineConstructorTest()
    {
        List<Vector3Int> cells = TestCells();

        RailLine line = new RailLine(7, cells);

        Assert.AreEqual(7, line.ID);
        Assert.AreEqual(cells[0], line.Start);
        Assert.AreEqual(cells[^1], line.End);
        Assert.AreEqual(3, line.Length);
        CollectionAssert.AreEqual(cells, line.Cells);
    }

    [Test]
    public void RailLineCellManipulationTest()
    {
        List<Vector3Int> cells = TestCells();

        RailLine line = new RailLine(1, cells);
        cells[0] = new Vector3Int(99, 99, 0);

        Assert.AreEqual(new Vector3Int(0, 0, 0), line.Cells[0]);
    }

    [Test]
    public void RailLineTrainAssignmentTest()
    {
        RailLine line = new RailLine(1, TestCells());

        Assert.NotNull(line.AssignedTrains);
        Assert.AreEqual(0, line.AssignedTrains.Count);
        Assert.AreEqual(2, line.MaxTrainCount);
        Assert.IsTrue(line.CanAddTrain);
    }
}
