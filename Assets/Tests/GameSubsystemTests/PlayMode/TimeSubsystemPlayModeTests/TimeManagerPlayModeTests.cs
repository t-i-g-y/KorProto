using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TimeManagerPlayModeTests
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
        if (timeGO != null)
            Object.DestroyImmediate(timeGO);
    }

    [UnityTest]
    public IEnumerator TimeAdvancesInPlayMode()
    {
        timeManager.SetSpeed(1f);
        int startHour = timeManager.HourCounter;
        yield return new WaitForSeconds(1.2f);

        Assert.Greater(timeManager.HourCounter, startHour);
    }

    [UnityTest]
    public IEnumerator PauseStopsTime()
    {
        timeManager.Pause();
        int startDay = timeManager.DayCounter;
        int startHour = timeManager.HourCounter;
        yield return new WaitForSeconds(1.2f);

        Assert.AreEqual(startDay, timeManager.DayCounter);
        Assert.AreEqual(startHour, timeManager.HourCounter);
    }

    [UnityTest]
    public IEnumerator SpeedMultiplierAcceleratesTime()
    {
        timeManager.SetSpeed(5f);
        int startHour = timeManager.HourCounter;
        yield return new WaitForSeconds(1.2f);

        Assert.GreaterOrEqual(timeManager.HourCounter, startHour + 5);
    }

    [UnityTest]
    public IEnumerator OnHourChangedFires()
    {
        bool eventFired = false;
        int eventDay = -1;
        int eventHour = -1;

        timeManager.OnHourChanged += (day, hour) =>
        {
            eventFired = true;
            eventDay = day;
            eventHour = hour;
        };

        timeManager.SetSpeed(1f);
        yield return new WaitForSeconds(1.2f);

        Assert.IsTrue(eventFired);
        Assert.AreEqual(timeManager.DayCounter, eventDay);
        Assert.AreEqual(timeManager.HourCounter, eventHour);
    }

    [UnityTest]
    public IEnumerator OnDayChangedFires()
    {
        bool eventFired = false;
        int eventDay = -1;
        timeManager.HourCounter = 23;

        timeManager.OnDayChanged += day =>
        {
            eventFired = true;
            eventDay = day;
        };

        timeManager.SetSpeed(1f);
        yield return new WaitForSeconds(1.2f);

        Assert.IsTrue(eventFired);
        Assert.AreEqual(1, eventDay);
        Assert.AreEqual(1, timeManager.DayCounter);
        Assert.AreEqual(0, timeManager.HourCounter);
    }

    [UnityTest]
    public IEnumerator UnpauseRestoresPreviousSpeed()
    {
        timeManager.SetSpeed(5f);
        timeManager.Pause();
        yield return new WaitForSeconds(0.2f);

        timeManager.Unpause();
        int startHour = timeManager.HourCounter;
        yield return new WaitForSeconds(1.2f);

        Assert.GreaterOrEqual(timeManager.HourCounter, startHour + 1);
    }
}
