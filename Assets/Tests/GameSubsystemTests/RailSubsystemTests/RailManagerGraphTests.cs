using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class RailManagerGraphTests
{
    private GameObject railManagerGO;
    private RailManager railManager;
    private readonly List<GameObject> trainObjects = new();

    [SetUp]
    public void SetUp()
    {
        railManagerGO = new GameObject();
        railManager = railManagerGO.AddComponent<RailManager>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject trainObject in trainObjects)
            Object.DestroyImmediate(trainObject);

        trainObjects.Clear();

        if (railManagerGO != null)
            Object.DestroyImmediate(railManagerGO);
    }

    [Test]
    public void CreateLineRailManagerTest()
    {
        RailLine line = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        Assert.AreEqual(1, railManager.Lines.Count);
        Assert.AreSame(line, railManager.Lines[0]);
        Assert.AreEqual(new Vector3Int(0, 0, 0), line.Start);
        Assert.AreEqual(new Vector3Int(1, 0, 0), line.End);
    }

    [Test]
    public void RailManagerGetLinesAtCellTest()
    {
        RailLine lineA = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        RailLine lineB = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0)
        });

        List<RailLine> result = railManager.GetLinesAtCell(new Vector3Int(1, 0, 0));

        Assert.AreEqual(2, result.Count);
        CollectionAssert.Contains(result, lineA);
        CollectionAssert.Contains(result, lineB);
    }

    [Test]
    public void GetTwinnedEndTest()
    {
        RailLine line = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        bool success = railManager.TryGetTwinnedEndpoint(line, new Vector3Int(0, 0, 0), out Vector3Int other);

        Assert.IsTrue(success);
        Assert.AreEqual(new Vector3Int(1, 0, 0), other);
    }

    [Test]
    public void GetTwinnedStartTest()
    {
        RailLine line = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        bool success = railManager.TryGetTwinnedEndpoint(line, new Vector3Int(1, 0, 0), out Vector3Int other);

        Assert.IsTrue(success);
        Assert.AreEqual(new Vector3Int(0, 0, 0), other);
    }

    [Test]
    public void ShortestPathSameCellTest()
    {
        Vector3Int cell = new Vector3Int(0, 0, 0);
        bool result = railManager.TryGetShortestPathFirstHop(cell, cell, out _, out _);

        Assert.IsFalse(result);
    }

    [Test]
    public void ShortestPathFakeCellTest()
    {
        RailLine lineA = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        ActivateTestIslands();
        AddOperationalTrainToLine(lineA);

        bool result = railManager.TryGetShortestPathFirstHop(new Vector3Int(0, 0, 0), new Vector3Int(10, 10, 0), out _, out _);

        Assert.IsFalse(result);
    }

    [Test]
    public void ShortestPathFirstHopTest()
    {
        RailLine lineA = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        RailLine lineB = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0)
        });

        ActivateTestIslands();

        AddOperationalTrainToLine(lineA);
        AddOperationalTrainToLine(lineB);

        bool result = railManager.TryGetShortestPathFirstHop(new Vector3Int(0, 0, 0), new Vector3Int(2, 0, 0), out Vector3Int firstHop, out float totalCost);

        Assert.IsTrue(result);
        Assert.AreEqual(new Vector3Int(1, 0, 0), firstHop);
        Assert.Greater(totalCost, 0f);
    }

    [Test]
    public void ShortestPathFirstHopCurrentLineTest()
    {
        RailLine lineA = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0)
        });

        RailLine lineB = railManager.CreateLine(new List<Vector3Int>
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(2, 0, 0)
        });

        ActivateTestIslands();

        AddOperationalTrainToLine(lineA);
        AddOperationalTrainToLine(lineB);

        bool result = railManager.IsFirstHopOnCurrentLine(lineA, new Vector3Int(0, 0, 0), new Vector3Int(2, 0, 0), out float totalCost);

        Assert.IsTrue(result);
        Assert.Greater(totalCost, 0f);
    }

    private Train AddOperationalTrainToLine(RailLine line)
    {
        GameObject trainGO = new GameObject($"Train for line {line.ID}");
        trainObjects.Add(trainGO);

        Train train = trainGO.AddComponent<Train>();

        train.Initialize(line, line.ID, ScriptableObject.CreateInstance<TrainConfig>());
        train.SetSpeedLevel(1);

        line.AddTrain(train);

        SetPrivateField(train, "isOperational", true);
        SetPrivateField(train, "isBroken", false);

        return train;
    }

    private void ActivateTestIslands()
    {
        FieldInfo islandsField = typeof(RailManager).GetField("islands", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo activeLinesField = typeof(RailManager).GetField("activeLines", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo buildConnectedCellsField = typeof(RailManager).GetField("buildConnectedCells", BindingFlags.Instance | BindingFlags.NonPublic);

        System.Collections.IDictionary islands = (System.Collections.IDictionary)islandsField.GetValue(railManager);
        HashSet<RailLine> activeLines = (HashSet<RailLine>)activeLinesField.GetValue(railManager);
        HashSet<Vector3Int> buildConnectedCells = (HashSet<Vector3Int>)buildConnectedCellsField.GetValue(railManager);

        activeLines.Clear();
        buildConnectedCells.Clear();

        foreach (object islandObject in islands.Values)
        {
            SetPrivateField(islandObject, "HasAnchor", true);
            SetPrivateField(islandObject, "IsCollapsed", false);

            var linesField = islandObject.GetType().GetField("Lines");
            var nodesField = islandObject.GetType().GetField("Nodes");

            var lines = linesField.GetValue(islandObject) as HashSet<RailLine>;
            var nodes = nodesField.GetValue(islandObject) as HashSet<Vector3Int>;

            foreach (RailLine line in lines)
                activeLines.Add(line);

            foreach (Vector3Int node in nodes)
                buildConnectedCells.Add(node);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName,BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.IsNotNull(field, $"{fieldName} was not found on {target.GetType().Name}");

        field.SetValue(target, value);
    }
}
