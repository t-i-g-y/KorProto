using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResearchSystemPlayModeTests
{
    private GameObject researchGO;
    private ResearchSystem researchSystem;
    private TechData techData;

    [SetUp]
    public void SetUp()
    {
        researchGO = new GameObject();
        researchSystem = researchGO.AddComponent<ResearchSystem>();

        techData = ScriptableObject.CreateInstance<TechData>();
        techData.ID = TechID.AllSpeedUpgrade;
        techData.techName = "AllSpeedUpgrade";
        techData.researchCost = 100;
        techData.prerequisites = new List<TechID>();

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", new List<TechData> { techData });
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");
    }

    [TearDown]
    public void TearDown()
    {
        if (researchGO != null)
            Object.DestroyImmediate(researchGO);

        if (techData != null)
            Object.DestroyImmediate(techData);
    }

    [UnityTest]
    public IEnumerator BuildTechnologyDatabase()
    {
        yield return null;
        Technology tech = researchSystem.GetTechnology(TechID.AllSpeedUpgrade);

        Assert.IsNotNull(tech);
        Assert.AreEqual(TechID.AllSpeedUpgrade, tech.Data.ID);
    }

    [UnityTest]
    public IEnumerator StartResearchSetsCurrentResearch()
    {
        yield return null;
        bool result = researchSystem.StartResearch(TechID.AllSpeedUpgrade);

        Assert.IsTrue(result);
        Assert.IsNotNull(researchSystem.CurrentResearch);
        Assert.AreEqual(TechID.AllSpeedUpgrade, researchSystem.CurrentResearch.Data.ID);
    }

    [UnityTest]
    public IEnumerator AddResearchPointsUnlocksTechnology()
    {
        yield return null;
        researchSystem.StartResearch(TechID.AllSpeedUpgrade);
        researchSystem.AddResearchPoints(100);
        Technology tech = researchSystem.GetTechnology(TechID.AllSpeedUpgrade);

        Assert.IsTrue(tech.IsUnlocked);
        Assert.IsNull(researchSystem.CurrentResearch);
    }

    [UnityTest]
    public IEnumerator TechnologyUnlockedEventFires()
    {
        yield return null;
        bool eventFired = false;
        Technology unlockedTech = null;

        researchSystem.OnTechnologyUnlocked += tech =>
        {
            eventFired = true;
            unlockedTech = tech;
        };

        researchSystem.StartResearch(TechID.AllSpeedUpgrade);
        researchSystem.AddResearchPoints(100);

        Assert.IsTrue(eventFired);
        Assert.IsNotNull(unlockedTech);
        Assert.AreEqual(TechID.AllSpeedUpgrade, unlockedTech.Data.ID);
    }
}
