using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FinanceSystemPlayModeTests
{
    private GameObject timeGO;
    private TimeManager timeManager;
    private GameObject financeGO;
    private FinanceSystem financeSystem;

    [SetUp]
    public void SetUp()
    {
        timeGO = new GameObject();
        timeManager = timeGO.AddComponent<TimeManager>();

        financeGO = new GameObject();
        financeSystem = financeGO.AddComponent<FinanceSystem>();

        TestImmitationHelper.SetPrivateField(financeSystem, "gameOverBalanceThreshold", -1000f);
        TestImmitationHelper.SetPrivateField(financeSystem, "maxDaysBelowThreshold", 5);
    }

    [TearDown]
    public void TearDown()
    {
        if (financeGO != null)
            Object.DestroyImmediate(financeGO);

        if (timeGO != null)
            Object.DestroyImmediate(timeGO);

        MenuPauseState.SetPaused(false);
    }

    [UnityTest]
    public IEnumerator FinanceSystemInitializes()
    {
        yield return null;

        Assert.AreEqual(100f, financeSystem.Balance);
        Assert.AreEqual(0f, financeSystem.LastBalanceChange);
        Assert.AreEqual(5, financeSystem.DaysLeftBeforeGameOver);
        Assert.IsFalse(financeSystem.IsInGameOverWarning);
    }

    [UnityTest]
    public IEnumerator WarningAfterFinanceTick()
    {
        financeSystem.AdjustBalance(-1200f);
        financeSystem.ApplyFinanceSystemTick();
        yield return null;

        Assert.IsTrue(financeSystem.IsInGameOverWarning);
        Assert.AreEqual(4, financeSystem.DaysLeftBeforeGameOver);
    }

    [UnityTest]
    public IEnumerator WarningCounterResetsBalanceRecovers()
    {
        financeSystem.AdjustBalance(-1200f);
        financeSystem.ApplyFinanceSystemTick();

        Assert.IsTrue(financeSystem.IsInGameOverWarning);

        financeSystem.AdjustBalance(1300f);
        yield return null;

        Assert.IsFalse(financeSystem.IsInGameOverWarning);
        Assert.AreEqual(5, financeSystem.DaysLeftBeforeGameOver);
    }

    [UnityTest]
    public IEnumerator GameOverPanelActivates()
    {
        GameObject panel = new GameObject("Game Over Panel");
        panel.SetActive(false);
        TestImmitationHelper.SetPrivateField(financeSystem, "gameOverPanel", panel);

        financeSystem.AdjustBalance(-1200f);

        for (int i = 0; i < 5; i++)
            financeSystem.ApplyFinanceSystemTick();

        yield return null;

        Assert.IsTrue(panel.activeSelf);
        Assert.IsTrue(timeManager.IsPaused);

        Object.DestroyImmediate(panel);
    }

    [UnityTest]
    public IEnumerator GameOverWarningEventFires()
    {
        bool eventFired = false;
        int daysLeft = -1;

        financeSystem.OnGameOverWarningChanged += value =>
        {
            eventFired = true;
            daysLeft = value;
        };

        financeSystem.AdjustBalance(-1200f);
        financeSystem.ApplyFinanceSystemTick();
        yield return null;

        Assert.IsTrue(eventFired);
        Assert.AreEqual(4, daysLeft);
    }
}