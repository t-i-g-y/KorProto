using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EconomyManagerPlayModeTests
{
    private GameObject timeGO;
    private TimeManager timeManager;
    private GameObject financeGO;
    private FinanceSystem financeSystem;
    private GameObject stationEconomyGO;
    private StationEconomySystem stationEconomySystem;
    private GameObject globalDemandGO;
    private GlobalDemandSystem globalDemandSystem;
    private GameObject economyGO;
    private EconomyManager economyManager;

    [SetUp]
    public void SetUp()
    {
        timeGO = new GameObject();
        timeManager = timeGO.AddComponent<TimeManager>();

        financeGO = new GameObject();
        financeSystem = financeGO.AddComponent<FinanceSystem>();

        stationEconomyGO = new GameObject();
        stationEconomySystem = stationEconomyGO.AddComponent<StationEconomySystem>();

        globalDemandGO = new GameObject();
        globalDemandSystem = globalDemandGO.AddComponent<GlobalDemandSystem>();

        economyGO = new GameObject();
        economyManager = economyGO.AddComponent<EconomyManager>();

        TestImmitationHelper.SetPrivateField(economyManager, "financeSystem", financeSystem);
        TestImmitationHelper.SetPrivateField(economyManager, "stationEconomySystem", stationEconomySystem);
        TestImmitationHelper.SetPrivateField(economyManager, "globalDemandSystem", globalDemandSystem);
    }

    [TearDown]
    public void TearDown()
    {
        if (economyGO != null)
            Object.DestroyImmediate(economyGO);

        if (globalDemandGO != null)
            Object.DestroyImmediate(globalDemandGO);

        if (stationEconomyGO != null)
            Object.DestroyImmediate(stationEconomyGO);

        if (financeGO != null)
            Object.DestroyImmediate(financeGO);

        if (timeGO != null)
            Object.DestroyImmediate(timeGO);
    }

    [UnityTest]
    public IEnumerator EconomyManagerDayChangedTriggersFinanceTick()
    {
        yield return null;

        Assert.AreEqual(0, financeSystem.CurrentDay);

        timeManager.AdvanceHoursForTests(24);
        yield return null;

        Assert.AreEqual(1, financeSystem.CurrentDay);
    }

    [UnityTest]
    public IEnumerator EconomyManagerDoesNotProcessSameDay()
    {
        yield return null;
        timeManager.AdvanceHoursForTests(24);
        yield return null;

        Assert.AreEqual(1, financeSystem.CurrentDay);

        economyManager.TickEconomy();

        Assert.AreEqual(2, financeSystem.CurrentDay);
    }

    [UnityTest]
    public IEnumerator EconomyManagerSaveDataStoresLastProcessedDay()
    {
        yield return null;
        timeManager.AdvanceHoursForTests(24);
        yield return null;
        EconomyManagerSaveData data = economyManager.GetSaveData();

        Assert.AreEqual(1, data.lastProcessedDay);
    }
}
