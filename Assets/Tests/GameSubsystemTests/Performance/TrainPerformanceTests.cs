using System.Collections.Generic;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

public class TrainPerformanceTests
{
    private GameObject timeGO;
    private TimeManager timeManager;
    private List<GameObject> trainObjects = new();
    private List<Train> trains = new();

    [SetUp]
    public void SetUp()
    {
        timeGO = new GameObject("Performance TimeManager");
        timeManager = timeGO.AddComponent<TimeManager>();
        timeManager.SetSpeed(1f);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject trainObject in trainObjects)
        {
            if (trainObject != null)
                Object.DestroyImmediate(trainObject);
        }

        trainObjects.Clear();
        trains.Clear();

        if (timeGO != null)
            Object.DestroyImmediate(timeGO);
    }

    [Test, Performance]
    public void Trains100Performance()
    {
        CreateTrains(100, 10);
        Measure.Method(() =>
        {
            for (int i = 0; i < trains.Count; i++)
                trains[i].HandleTrainMovementForTests();
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }

    [Test, Performance]
    public void Trains500Performance()
    {
        CreateTrains(500, 10);
        Measure.Method(() =>
        {
            for (int i = 0; i < trains.Count; i++)
                trains[i].HandleTrainMovementForTests();
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }

    [Test, Performance]
    public void Trains1000Performance()
    {
        CreateTrains(1000, 10);
        Measure.Method(() =>
        {
            for (int i = 0; i < trains.Count; i++)
                trains[i].HandleTrainMovementForTests();
        })
        .WarmupCount(5)
        .MeasurementCount(30)
        .Run();
    }

    private void CreateTrains(int trainCount, int wagonCount)
    {
        for (int i = 0; i < trainCount; i++)
        {
            GameObject trainGO = new GameObject($"Performance Train {i}");
            trainObjects.Add(trainGO);

            TrainConsist consist = trainGO.AddComponent<TrainConsist>();
            Train train = trainGO.AddComponent<Train>();

            consist.Initialize();
            consist.SetMaxWagonCount(wagonCount);
            for (int w = 0; w < wagonCount; w++)
                consist.TryAddWagon();

            RailLine line = new RailLine(
                i,
                new List<Vector3Int>
                {
                    new Vector3Int(0, i, 0),
                    new Vector3Int(1, i, 0),
                    new Vector3Int(2, i, 0)
                }
            );

            TrainConfig config = ScriptableObject.CreateInstance<TrainConfig>();
            train.Initialize(line, i, config);

            train.SetPath(
                line.Cells,
                new List<Vector3>
                {
                    new Vector3(0f, i, 0f),
                    new Vector3(5f, i, 0f),
                    new Vector3(10f, i, 0f)
                }
            );

            train.SetOperationalForTests(true);
            train.SetBrokenForTests(false);
            TestImmitationHelper.SetPrivateField(train, "breakChancePerSecond", 0f);
            train.enabled = false;
            trains.Add(train);
        }
    }
}
