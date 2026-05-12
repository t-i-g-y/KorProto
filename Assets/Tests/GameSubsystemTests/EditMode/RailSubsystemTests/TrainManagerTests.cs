using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;



public class TrainManagerTests
{
    private GameObject managerGO;
    private TrainManager trainManager;

    private GameObject trainAGO;
    private GameObject trainBGO;

    private Train trainA;
    private Train trainB;

    [SetUp]
    public void SetUp()
    {
        managerGO = new GameObject();
        trainManager = managerGO.AddComponent<TrainManager>();

        trainAGO = new GameObject();
        trainA = trainAGO.AddComponent<Train>();
        trainAGO.AddComponent<TrainConsist>();

        trainBGO = new GameObject();
        trainB = trainBGO.AddComponent<Train>();
        trainBGO.AddComponent<TrainConsist>();

        RailLine lineA = new RailLine(
            1, 
            new List<Vector3Int>
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0)
            });

        RailLine lineB = new RailLine(
            2, new List<Vector3Int>
            {
                new Vector3Int(2, 0, 0),
                new Vector3Int(3, 0, 0)
            });

        trainA.Initialize(lineA, 11, ScriptableObject.CreateInstance<TrainConfig>());
        trainB.Initialize(lineB, 12, ScriptableObject.CreateInstance<TrainConfig>());

        trainA.SetPath(
            lineA.Cells,
            new List<Vector3> 
            { 
                Vector3.zero, 
                Vector3.right 
            });

        trainB.SetPath(
            lineB.Cells,
            new List<Vector3> 
            { 
                new Vector3(2f, 0f, 0f), 
                new Vector3(3f, 0f, 0f) 
            });

        trainManager.Trains.Add(trainA);
        trainManager.Trains.Add(trainB);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(trainAGO);
        Object.DestroyImmediate(trainBGO);
        Object.DestroyImmediate(managerGO);
    }

    [Test]
    public void GetBrokenTrainsTest()
    {
        TrainSaveData brokenData = trainB.GetSaveData();
        brokenData.isBroken = true;
        trainB.LoadFromSaveData(brokenData);

        List<Train> broken = trainManager.GetBrokenTrains();

        Assert.AreEqual(1, broken.Count);
        Assert.AreSame(trainB, broken[0]);
    }

    [Test]
    public void TrainSelectionTest()
    {
        trainManager.ToggleSelection(trainA);

        Assert.AreSame(trainA, trainManager.SelectedTrain);
    }

    [Test]
    public void TrainDeselectionTest()
    {
        trainManager.ToggleSelection(trainA);
        trainManager.ToggleSelection(trainA);

        Assert.IsNull(trainManager.SelectedTrain);
    }

    [Test]
    public void TrainSelectionChangesPreviousSelectionTest()
    {
        trainManager.ToggleSelection(trainA);
        trainManager.ToggleSelection(trainB);

        Assert.AreSame(trainB, trainManager.SelectedTrain);
    }
}
