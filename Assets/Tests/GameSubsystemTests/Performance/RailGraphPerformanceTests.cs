using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using System.Reflection;

public class RailGraphPerformanceTests
{
    private GameObject railManagerGO;
    private RailManager railManager;

    [SetUp]
    public void SetUp()
    {
        railManagerGO = new GameObject();
        railManager = railManagerGO.AddComponent<RailManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(railManagerGO);
    }

    [Test, Performance]
    public void ShortestPath50LinesPerformance()
    {
        for (int i = 0; i < 50; i++)
        {
            railManager.CreateLine(new List<Vector3Int>
            {
                new Vector3Int(i, 0, 0),
                new Vector3Int(i + 1, 0, 0)
            });
        }

        ActivateTestIslands();
        
        Measure.Method(() =>
        {
            railManager.TryGetShortestPathFirstHop(
                new Vector3Int(0, 0, 0),
                new Vector3Int(50, 0, 0),
                out _,
                out _
            );
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }

    [Test, Performance]
    public void ShortestPath200LinesPerformance()
    {
        for (int i = 0; i < 200; i++)
        {
            railManager.CreateLine(new List<Vector3Int>
            {
                new Vector3Int(i, 0, 0),
                new Vector3Int(i + 1, 0, 0)
            });
        }

        ActivateTestIslands();

        Measure.Method(() =>
        {
            railManager.TryGetShortestPathFirstHop(
                new Vector3Int(0, 0, 0),
                new Vector3Int(200, 0, 0),
                out _,
                out _
            );
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }

    [Test, Performance]
    public void ShortestPath1000LinesPerformance()
    {
        for (int i = 0; i < 1000; i++)
        {
            railManager.CreateLine(new List<Vector3Int>
            {
                new Vector3Int(i, 0, 0),
                new Vector3Int(i + 1, 0, 0)
            });
        }

        ActivateTestIslands();

        Measure.Method(() =>
        {
            railManager.TryGetShortestPathFirstHop(
                new Vector3Int(0, 0, 0),
                new Vector3Int(1000, 0, 0),
                out _,
                out _
            );
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
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
            TestImmitationHelper.SetPrivateField(islandObject, "HasAnchor", true);
            TestImmitationHelper.SetPrivateField(islandObject, "IsCollapsed", false);

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
}
