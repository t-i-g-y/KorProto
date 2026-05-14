using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

public class SavePerformanceTests
{
    [Test, Performance]
    public void LargeRailSavePerformance()
    {
        SubsystemSaveData data = new SubsystemSaveData
        {
            railData = new RailManagerSaveData
            {
                nextID = 1000,
                selectedLineID = -1,
                lines = new List<RailLineSaveData>()
            }
        };

        for (int i = 0; i < 1000; i++)
        {
            data.railData.lines.Add(new RailLineSaveData
            {
                ID = i,
                cells = new List<Vector3Int>
                {
                    new Vector3Int(i, 0, 0),
                    new Vector3Int(i + 1, 0, 0),
                    new Vector3Int(i + 2, 0, 0)
                }
            });
        }

        Measure.Method(() =>
        {
            JsonUtility.ToJson(data);
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }

    [Test, Performance]
    public void LargeRailLoadPerformance()
    {
        SubsystemSaveData data = new SubsystemSaveData
        {
            railData = new RailManagerSaveData
            {
                nextID = 1000,
                selectedLineID = -1,
                lines = new List<RailLineSaveData>()
            }
        };

        for (int i = 0; i < 1000; i++)
        {
            data.railData.lines.Add(new RailLineSaveData
            {
                ID = i,
                cells = new List<Vector3Int>
                {
                    new Vector3Int(i, 0, 0),
                    new Vector3Int(i + 1, 0, 0),
                    new Vector3Int(i + 2, 0, 0)
                }
            });
        }

        string json = JsonUtility.ToJson(data);

        Measure.Method(() =>
        {
            JsonUtility.FromJson<SubsystemSaveData>(json);
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }
}