using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TrainManagerPlayModeTests
{
    private GameObject timeManagerGO;
    private TimeManager timeManager;
    private GameObject managerGO;
    private TrainManager trainManager;
    private GameObject trainAGO;
    private GameObject trainBGO;
    private Train trainA;
    private Train trainB;
    private RailLine lineA;
    private RailLine lineB;

    [SetUp]
    public void SetUp()
    {
        timeManagerGO = new GameObject();
        timeManager = timeManagerGO.AddComponent<TimeManager>();

        managerGO = new GameObject();
        trainManager = managerGO.AddComponent<TrainManager>();

        lineA = new RailLine(
            1,
            new List<Vector3Int>
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(1, 0, 0)
            }
        );

        lineB = new RailLine(
            2,
            new List<Vector3Int>
            {
                new Vector3Int(2, 0, 0),
                new Vector3Int(3, 0, 0)
            }
        );

        trainA = CreateTrain("Train A", lineA, 11, Vector3.zero, Vector3.right * 5f, out trainAGO);
        trainB = CreateTrain("Train B", lineB, 12, new Vector3(2f, 0f, 0f), new Vector3(7f, 0f, 0f), out trainBGO);

        trainManager.Trains.Add(trainA);
        trainManager.Trains.Add(trainB);
    }

    [TearDown]
    public void TearDown()
    {
        if (trainAGO != null)
            Object.DestroyImmediate(trainAGO);

        if (trainBGO != null)
            Object.DestroyImmediate(trainBGO);

        if (managerGO != null)
            Object.DestroyImmediate(managerGO);

        if (timeManagerGO != null)
            Object.DestroyImmediate(timeManagerGO);
    }

    private Train CreateTrain(string name, RailLine line, int id, Vector3 start, Vector3 end, out GameObject trainGO)
    {
        trainGO = new GameObject(name);

        TrainConsist consist = trainGO.AddComponent<TrainConsist>();
        Train train = trainGO.AddComponent<Train>();

        consist.Initialize();
        TrainConfig config = ScriptableObject.CreateInstance<TrainConfig>();

        train.Initialize(line, id, config);
        train.SetPath(
            line.Cells,
            new List<Vector3>
            {
                start,
                end
            }
        );

        train.SetOperationalForTests(true);
        train.SetBrokenForTests(false);

        TestImmitationHelper.SetPrivateField(train, "breakChancePerSecond", 0f);
        train.enabled = false;

        line.AddTrain(train);
        return train;
    }

    [UnityTest]
    public IEnumerator ToggleSelectionSelectsTrain()
    {
        trainManager.ToggleSelection(trainA);
        yield return null;

        Assert.AreSame(trainA, trainManager.SelectedTrain);
    }

    [UnityTest]
    public IEnumerator ToggleSelectionDeselectsAlreadySelectedTrain()
    {
        trainManager.ToggleSelection(trainA);
        yield return null;

        trainManager.ToggleSelection(trainA);
        yield return null;

        Assert.IsNull(trainManager.SelectedTrain);
    }

    [UnityTest]
    public IEnumerator ToggleSelectionReplacesPreviousSelection()
    {
        trainManager.ToggleSelection(trainA);
        yield return null;

        trainManager.ToggleSelection(trainB);
        yield return null;

        Assert.AreSame(trainB, trainManager.SelectedTrain);
    }

    [UnityTest]
    public IEnumerator ForceDeselectClearsSelectedTrain()
    {
        trainManager.ToggleSelection(trainA);
        yield return null;

        trainManager.ForceDeselect(trainA);
        yield return null;

        Assert.IsNull(trainManager.SelectedTrain);
    }

    [UnityTest]
    public IEnumerator GetBrokenTrainsReturnsOnlyBrokenTrains()
    {
        trainA.SetBrokenForTests(false);
        trainB.SetBrokenForTests(true);
        yield return null;

        List<Train> brokenTrains = trainManager.GetBrokenTrains();

        Assert.AreEqual(1, brokenTrains.Count);
        Assert.AreSame(trainB, brokenTrains[0]);
    }

    [UnityTest]
    public IEnumerator RepairRemovesTrainFromBrokenList()
    {
        trainB.SetBrokenForTests(true);
        yield return null;

        Assert.AreEqual(1, trainManager.GetBrokenTrains().Count);

        trainB.Repair();
        yield return null;

        Assert.AreEqual(0, trainManager.GetBrokenTrains().Count);
    }

    [UnityTest]
    public IEnumerator RemoveTrainRemovesFromManagerAndLine()
    {
        Assert.Contains(trainA, trainManager.Trains);
        Assert.Contains(trainA, lineA.AssignedTrains);

        trainManager.RemoveTrain(trainA);
        yield return null;

        Assert.IsFalse(trainManager.Trains.Contains(trainA));
        Assert.IsFalse(lineA.AssignedTrains.Contains(trainA));
    }

    [UnityTest]
    public IEnumerator GetSaveDataStoresSelectedTrainAndTrainList()
    {
        trainManager.ToggleSelection(trainB);
        yield return null;

        TrainManagerSaveData data = trainManager.GetSaveData();

        Assert.AreEqual(12, data.selectedTrainID);
        Assert.AreEqual(2, data.trains.Count);
    }
}