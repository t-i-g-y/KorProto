using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TrainPlayModeTests
{
    private GameObject timeManagerGO;
    private TimeManager timeManager;
    private GameObject trainGO;
    private Train train;
    private TrainConsist consist;
    private RailLine line;
    private TrainConfig config;

    [SetUp]
    public void SetUp()
    {
        timeManagerGO = new GameObject();
        timeManager = timeManagerGO.AddComponent<TimeManager>();
        trainGO = new GameObject();
        consist = trainGO.AddComponent<TrainConsist>();
        train = trainGO.AddComponent<Train>();
        consist.Initialize();

        line = new RailLine(
            1,
            new List<Vector3Int>
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0)
            }
        );

        config = ScriptableObject.CreateInstance<TrainConfig>();

        train.Initialize(line, 10, config);
        train.SetPath(
            line.Cells,
            new List<Vector3>
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(5f, 0f, 0f)
            }
        );

        train.SetOperationalForTests(true);
        train.SetBrokenForTests(false);
        TestImmitationHelper.SetPrivateField(train, "breakChancePerSecond", 0f);

    }

    [TearDown]
    public void TearDown()
    {
        if (trainGO != null)
            Object.DestroyImmediate(trainGO);

        if (config != null)
            Object.DestroyImmediate(config);
        
        if (timeManagerGO != null)
            Object.DestroyImmediate(timeManagerGO);
    }

    [UnityTest]
    public IEnumerator TrainMovementHandled()
    {
        Vector3 startPosition = train.transform.position;

        for (int i = 0; i < 20; i++)
        {
            train.HandleTrainMovementForTests();
            yield return null;
        }

        Vector3 endPosition = train.transform.position;

        Assert.AreNotEqual(startPosition, endPosition);
        Assert.Greater(endPosition.x, startPosition.x);
    }

    [UnityTest]
    public IEnumerator BrokenTrainDoesNotMove()
    {
        train.SetBrokenForTests(true);
        Vector3 startPosition = train.transform.position;
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(startPosition, train.transform.position);
    }

    [UnityTest]
    public IEnumerator RepairRestoresMovement()
    {
        train.SetBrokenForTests(true);

        for (int i = 0; i < 5; i++)
        {
            train.HandleTrainMovementForTests();
            yield return null;
        }

        Vector3 frozenPosition = train.transform.position;

        train.Repair();

        for (int i = 0; i < 20; i++)
        {
            train.HandleTrainMovementForTests();
            yield return null;
        }

        Assert.AreNotEqual(frozenPosition, train.transform.position);
    }

    [UnityTest]
    public IEnumerator TrainConsistInitializesLocomotive()
    {
        yield return null;

        Assert.IsNotNull(consist.headLocomotive);
        Assert.AreEqual(6, consist.headLocomotive.capacity);
        Assert.AreEqual(6, consist.totalCapacity);
    }

    [UnityTest]
    public IEnumerator AddingWagonIncreasesCapacity()
    {
        int capacityBefore = consist.totalCapacity;
        bool result = consist.TryAddWagon();
        yield return null;

        Assert.IsTrue(result);
        Assert.AreEqual(capacityBefore + 6, consist.totalCapacity);
        Assert.AreEqual(1, consist.WagonCount);
    }

    [UnityTest]
    public IEnumerator TrainReversesAtEndOfLine()
    {
        timeManager.SetSpeed(20f);
        float timeout = Time.time + 10f;
        bool reversed = false;

        while (Time.time < timeout)
        {
            if (train.GetSaveData().dir < 0)
            {
                reversed = true;
                break;
            }
            yield return null;
        }

        Assert.IsTrue(reversed);
    }

    [UnityTest]
    public IEnumerator TrainMovesBackwardAfterReversing()
    {
        timeManager.SetSpeed(20f);
        float timeout = Time.time + 10f;

        while (Time.time < timeout)
        {
            if (train.GetSaveData().dir < 0)
                break;

            yield return null;
        }

        Assert.AreEqual(-1, train.GetSaveData().dir);

        float xAfterReverse = train.transform.position.x;
        yield return new WaitForSeconds(0.1f);

        Assert.Less(train.transform.position.x, xAfterReverse);
    }
}