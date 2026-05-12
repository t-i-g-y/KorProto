using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResearchIncomeSystemPlayModeTests
{
    private GameObject timeGO;
    private TimeManager timeManager;
    private GameObject researchGO;
    private ResearchSystem researchSystem;
    private GameObject incomeGO;
    private ResearchIncomeSystem incomeSystem;
    private TechData techData;

    [SetUp]
    public void SetUp()
    {
        timeGO = new GameObject();
        timeManager = timeGO.AddComponent<TimeManager>();

        researchGO = new GameObject();
        researchSystem = researchGO.AddComponent<ResearchSystem>();

        techData = ScriptableObject.CreateInstance<TechData>();
        techData.ID = TechID.AllSpeedUpgrade;
        techData.techName = "AllSpeedUpgrade";
        techData.researchCost = 10;
        techData.prerequisites = new List<TechID>();

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", new List<TechData> { techData } );
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");

        incomeGO = new GameObject();
        incomeSystem = incomeGO.AddComponent<ResearchIncomeSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        if (incomeGO != null)
            Object.DestroyImmediate(incomeGO);

        if (researchGO != null)
            Object.DestroyImmediate(researchGO);

        if (timeGO != null)
            Object.DestroyImmediate(timeGO);

        if (techData != null)
            Object.DestroyImmediate(techData);
    }

    [UnityTest]
    public IEnumerator HourTickAddsResearchProgress()
    {
        yield return null;
        researchSystem.StartResearch(TechID.AllSpeedUpgrade);
        float progressBefore = researchSystem.GetTechnology(TechID.AllSpeedUpgrade).Progress;

        timeManager.AdvanceHoursForTests();
        yield return null;
        float progressAfter = researchSystem.GetTechnology(TechID.AllSpeedUpgrade).Progress;

        Assert.Greater(progressAfter, progressBefore);
    }

    [UnityTest]
    public IEnumerator HourTickCanUnlockTechnology()
    {
        yield return null;
        researchSystem.StartResearch(TechID.AllSpeedUpgrade);

        for (int i = 0; i < 20; i++)
            timeManager.AdvanceHoursForTests();

        yield return null;

        Assert.IsTrue(researchSystem.GetTechnology(TechID.AllSpeedUpgrade).IsUnlocked);
    }
}
