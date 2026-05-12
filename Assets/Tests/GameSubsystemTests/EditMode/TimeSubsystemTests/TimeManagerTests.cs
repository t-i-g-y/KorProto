using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TimeManagerTests
{
    private GameObject timeGO;
    private TimeManager timeManager;

    [SetUp]
    public void SetUp()
    {
        timeGO = new GameObject();
        timeManager = timeGO.AddComponent<TimeManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(timeGO);
    }

    [Test]
    public void InitialTimeTest()
    {
        Assert.AreEqual(0, timeManager.DayCounter);
        Assert.AreEqual(0, timeManager.HourCounter);
        Assert.AreEqual("День 0 00:00", timeManager.DayHourString);
    }

    [Test]
    public void HourCounterDayTransitionTest()
    {
        timeManager.HourCounter = 25;

        Assert.AreEqual(1, timeManager.DayCounter);
        Assert.AreEqual(1, timeManager.HourCounter);
        Assert.AreEqual("День 1 01:00", timeManager.DayHourString);
    }

    [Test]
    public void PauseTest()
    {
        timeManager.Pause();

        Assert.AreEqual(0f, timeManager.CustomDeltaTime);
    }

    [Test]
    public void SetSpeedNegativeValueTest()
    {
        timeManager.SetSpeed(-5f);

        Assert.AreEqual(0f, timeManager.CustomDeltaTime);
    }

    [Test]
    public void AdvanceOneHourTest()
    {
        timeManager.AdvanceHoursForTests();

        Assert.AreEqual(1, timeManager.HourCounter);
        Assert.AreEqual(0, timeManager.DayCounter);
    }

    [Test]
    public void AdvanceOneHourDayTest()
    {
        timeManager.HourCounter = 23;

        timeManager.AdvanceHoursForTests();

        Assert.AreEqual(0, timeManager.HourCounter);
        Assert.AreEqual(1, timeManager.DayCounter);
    }

    [Test]
    public void Advance24HoursTest()
    {
        timeManager.AdvanceHoursForTests(24);

        Assert.AreEqual(1, timeManager.DayCounter);
        Assert.AreEqual(0, timeManager.HourCounter);
    }

    [Test]
    public void Advance25HoursTest()
    {
        timeManager.AdvanceHoursForTests(25);

        Assert.AreEqual(1, timeManager.DayCounter);
        Assert.AreEqual(1, timeManager.HourCounter);
    }

    [Test]
    public void HourChangedEventTest()
    {
        bool called = false;
        int day = -1;
        int hour = -1;

        timeManager.OnHourChanged += (d, h) =>
        {
            called = true;
            day = d;
            hour = h;
        };

        timeManager.AdvanceHoursForTests();

        Assert.IsTrue(called);
        Assert.AreEqual(0, day);
        Assert.AreEqual(1, hour);
    }

    [Test]
    public void DayChangedEventTest()
    {
        bool called = false;
        int day = -1;

        timeManager.HourCounter = 23;

        timeManager.OnDayChanged += d =>
        {
            called = true;
            day = d;
        };

        timeManager.AdvanceHoursForTests();

        Assert.IsTrue(called);
        Assert.AreEqual(1, day);
    }
}
