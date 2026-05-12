using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResearchSystemTests
{
    private GameObject researchGO;
    private ResearchSystem researchSystem;
    private TechData baseTech;
    private TechData dependentTech;

    [SetUp]
    public void SetUp()
    {
        researchGO = new GameObject();
        researchSystem = researchGO.AddComponent<ResearchSystem>();

        baseTech = CreateTech(TechID.BaseTrainCost, 100);
        dependentTech = CreateTech(TechID.WagonCapacity, 50);
        dependentTech.prerequisites.Add(TechID.BaseTrainCost);

        List<TechData> database = new List<TechData>
        {
            baseTech,
            dependentTech
        };

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", database);
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(researchGO);
        Object.DestroyImmediate(baseTech);
        Object.DestroyImmediate(dependentTech);
    }

    private TechData CreateTech(TechID id, int cost)
    {
        TechData data = ScriptableObject.CreateInstance<TechData>();
        data.ID = id;
        data.techName = id.ToString();
        data.researchCost = cost;
        data.prerequisites = new List<TechID>();
        return data;
    }

    [Test]
    public void BuildRuntimeTechnologiesTest()
    {
        Assert.IsNotNull(researchSystem.GetTechnology(TechID.BaseTrainCost));
        Assert.IsNotNull(researchSystem.GetTechnology(TechID.WagonCapacity));
        Assert.AreEqual(2, researchSystem.GetAllTechnologies().Count);
    }

    [Test]
    public void CanResearchNoPrereqTest()
    {
        Assert.IsTrue(researchSystem.CanResearch(TechID.BaseTrainCost));
    }

    [Test]
    public void CanResearchLockedPrereqTest()
    {
        Assert.IsFalse(researchSystem.CanResearch(TechID.WagonCapacity));
    }

    [Test]
    public void StartResearchTest()
    {
        bool result = researchSystem.StartResearch(TechID.BaseTrainCost);

        Assert.IsTrue(result);
        Assert.IsNotNull(researchSystem.CurrentResearch);
        Assert.AreEqual(TechID.BaseTrainCost, researchSystem.CurrentResearch.Data.ID);
        Assert.IsTrue(researchSystem.CurrentResearch.IsResearching);
    }

    [Test]
    public void StartResearchLockedTechTest()
    {
        bool result = researchSystem.StartResearch(TechID.WagonCapacity);

        Assert.IsFalse(result);
        Assert.IsNull(researchSystem.CurrentResearch);
    }

    [Test]
    public void StopResearchTest()
    {
        researchSystem.StartResearch(TechID.BaseTrainCost);

        researchSystem.StopResearch();

        Assert.IsNull(researchSystem.CurrentResearch);
        Assert.IsFalse(researchSystem.GetTechnology(TechID.BaseTrainCost).IsResearching);
    }

    [Test]
    public void AddResearchPointsTest()
    {
        researchSystem.StartResearch(TechID.BaseTrainCost);

        researchSystem.AddResearchPoints(25);

        Assert.AreEqual(25f, researchSystem.GetTechnology(TechID.BaseTrainCost).Progress);
    }

    [Test]
    public void AddResearchPointNoCurrentTechTest()
    {
        researchSystem.AddResearchPoints(25);

        Assert.AreEqual(0f, researchSystem.GetTechnology(TechID.BaseTrainCost).Progress);
    }

    [Test]
    public void AddResearchUnlockTechnologyTest()
    {
        researchSystem.StartResearch(TechID.BaseTrainCost);

        researchSystem.AddResearchPoints(100);

        Technology tech = researchSystem.GetTechnology(TechID.BaseTrainCost);

        Assert.IsTrue(tech.IsUnlocked);
        Assert.IsNull(researchSystem.CurrentResearch);
    }

    [Test]
    public void UnlockTechnologyAllowsDependentTest()
    {
        researchSystem.StartResearch(TechID.BaseTrainCost);
        researchSystem.AddResearchPoints(100);

        Assert.IsTrue(researchSystem.CanResearch(TechID.WagonCapacity));
    }

    [Test]
    public void NewResearchStopsPreviousResearchTest()
    {
        TechData secondBaseTech = CreateTech(TechID.BaseRailMaintenance, 40);

        List<TechData> database = new List<TechData>
        {
            baseTech,
            dependentTech,
            secondBaseTech
        };

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", database);
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");

        researchSystem.StartResearch(TechID.BaseTrainCost);
        researchSystem.StartResearch(TechID.BaseRailMaintenance);

        Assert.IsFalse(researchSystem.GetTechnology(TechID.BaseTrainCost).IsResearching);
        Assert.IsTrue(researchSystem.GetTechnology(TechID.BaseRailMaintenance).IsResearching);

        Object.DestroyImmediate(secondBaseTech);
    }
}
