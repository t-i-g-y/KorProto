using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TrainTests
{
    private GameObject trainGO;
    private Train train;
    private RailLine line;
    private TrainConfig config;

    [SetUp]
    public void SetUp()
    {
        trainGO = new GameObject("Test Train");
        train = trainGO.AddComponent<Train>();
        trainGO.AddComponent<TrainConsist>();

        line = new RailLine(
            66,
            new List<Vector3Int>
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(2, 0, 0)
            }
        );

        config = ScriptableObject.CreateInstance<TrainConfig>();

        train.Initialize(line, 111, config);

        train.SetPath(
            new List<Vector3Int>
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(2, 0, 0)
            },
            new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f)
            }
        );
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(trainGO);
    }

    [Test]
    public void TrainInitializeTest()
    {
        Assert.AreEqual(111, train.ID);
        Assert.AreSame(line, train.AssignedLine);
    }

    [Test]
    public void SetPathMovesTrainTest()
    {
        Assert.AreEqual(new Vector3(0f, 0f, 0f), train.transform.position);
    }

    [Test]
    public void TrainRepairTest()
    {
        TrainSaveData data = train.GetSaveData();
        data.isBroken = true;

        train.LoadFromSaveData(data);

        Assert.IsTrue(train.IsBroken);

        train.Repair();

        Assert.IsFalse(train.IsBroken);
    }
}
