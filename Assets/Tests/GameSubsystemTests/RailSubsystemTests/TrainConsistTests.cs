using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TrainConsistTests
{
    private GameObject consistGO;
    private TrainConsist consist;

    [SetUp]
    public void SetUp()
    {
        consistGO = new GameObject();
        consist = consistGO.AddComponent<TrainConsist>();
        consist.Initialize();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(consistGO);
    }

    [Test]
    public void TrainConsistUnitConstructorTest()
    {
        TrainConsistUnit unit = new TrainConsistUnit(6, 1.5f);

        Assert.AreEqual(6, unit.capacity);
        Assert.AreEqual(1.5f, unit.maintenance);
    }

    [Test]
    [TestCase(-5, 1f)]
    [TestCase(-10, 10f)]
    [TestCase(-4, -4f)]
    [TestCase(0, 1f)]
    public void TrainConsistUnitNegativeCapacityConstructorTest(int capacity, float maintenance)
    {
        TrainConsistUnit unit = new TrainConsistUnit(capacity, maintenance);

        Assert.AreEqual(0, unit.capacity);
    }

    [Test]
    [TestCase(10, -1f)]
    [TestCase(-4, -0.000001f)]
    [TestCase(-4, -4f)]
    [TestCase(0, 0f)]
    public void TrainConsistUnitNegativeMaintenanceConstructorTest(int capacity, float maintenance)
    {
        TrainConsistUnit unit = new TrainConsistUnit(capacity, maintenance);

        Assert.AreEqual(0f, unit.maintenance);
    }

    [Test]
    public void DefaultHeadLocomotiveTest()
    {
        Assert.IsNotNull(consist.headLocomotive);
        Assert.AreEqual(6, consist.headLocomotive.capacity);
        Assert.AreEqual(6, consist.totalCapacity);
        Assert.AreEqual(0, consist.usedCapacity);
    }

    [Test]
    public void AddWagonCapacityTest()
    {
        int capacityBefore = consist.totalCapacity;

        bool result = consist.TryAddWagon();

        Assert.IsTrue(result);
        Assert.AreEqual(1, consist.WagonCount);
        Assert.AreEqual(capacityBefore + 6, consist.totalCapacity);
    }

    [Test]
    public void RemoveWagonCapacityTest()
    {
        consist.TryAddWagon();
        consist.TryAddWagon();

        Assert.AreEqual(2, consist.WagonCount);
        Assert.AreEqual(18, consist.totalCapacity);

        consist.RemoveLastWagon();

        Assert.AreEqual(1, consist.WagonCount);
        Assert.AreEqual(12, consist.totalCapacity);
    }

    [Test]
    public void RemoveWagonWhenNoWagonTest()
    {
        consist.RemoveLastWagon();

        Assert.AreEqual(0, consist.WagonCount);
        Assert.AreEqual(6, consist.totalCapacity);
    }

    [Test]
    public void ChangeHeadCapacityTest()
    {
        consist.ChangeHeadCapacity(4);

        Assert.AreEqual(10, consist.headLocomotive.capacity);
        Assert.AreEqual(10, consist.totalCapacity);
    }

    [Test]
    public void ChangeHeadCapacityNegativeTest()
    {
        consist.ChangeHeadCapacity(-100);

        Assert.AreEqual(0, consist.headLocomotive.capacity);
        Assert.AreEqual(0, consist.totalCapacity);
    }

    [Test]
    public void ChangeHeadMaintenanceTest()
    {
        consist.ChangeHeadMaintenance(0.5f);

        Assert.AreEqual(0.5f, consist.headLocomotive.maintenance);
    }

    [Test]
    public void ChangeHeadMaintenanceNegativeTest()
    {
        consist.ChangeHeadMaintenance(-10f);

        Assert.AreEqual(0f, consist.headLocomotive.maintenance);
    }

    [Test]
    public void GetHeadCapacityTest()
    {
        Assert.AreEqual(6, consist.GetHeadCapacity());

        consist.ChangeHeadCapacity(2);

        Assert.AreEqual(8, consist.GetHeadCapacity());
    }

    [Test]
    public void GetUnitCapacityWagonTest()
    {
        consist.TryAddWagon();

        Assert.AreEqual(6, consist.GetUnitCapacity(0));
    }

    [Test]
    public void GetUnitCapacityInvalidWagonTest()
    {
        Assert.AreEqual(0, consist.GetUnitCapacity(-1));
        Assert.AreEqual(0, consist.GetUnitCapacity(0));
    }


    [Test]
    public void BuildCargoSliceNoCargoTest()
    {
        ResourceAmount[] slice = consist.BuildCargoSlice(0, 6);

        Assert.IsNotNull(slice);
        Assert.AreEqual(System.Enum.GetValues(typeof(ResourceType)).Length, slice.Length);

        foreach (ResourceAmount amount in slice)
            Assert.AreEqual(0, amount.Amount);
    }
}
