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

        baseTech = CreateTech(TechID.TrainSpeed, 100);
        dependentTech = CreateTech(TechID.CapacityUpgrade, 50);
        dependentTech.prerequisites.Add(TechID.TrainSpeed);

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
        Assert.IsNotNull(researchSystem.GetTechnology(TechID.TrainSpeed));
        Assert.IsNotNull(researchSystem.GetTechnology(TechID.CapacityUpgrade));
        Assert.AreEqual(2, researchSystem.GetAllTechnologies().Count);
    }

    [Test]
    public void CanResearchNoPrereqTest()
    {
        Assert.IsTrue(researchSystem.CanResearch(TechID.TrainSpeed));
    }

    [Test]
    public void CanResearchLockedPrereqTest()
    {
        Assert.IsFalse(researchSystem.CanResearch(TechID.CapacityUpgrade));
    }

    [Test]
    public void StartResearchTest()
    {
        bool result = researchSystem.StartResearch(TechID.TrainSpeed);

        Assert.IsTrue(result);
        Assert.IsNotNull(researchSystem.CurrentResearch);
        Assert.AreEqual(TechID.TrainSpeed, researchSystem.CurrentResearch.Data.ID);
        Assert.IsTrue(researchSystem.CurrentResearch.IsResearching);
    }

    [Test]
    public void StartResearchLockTechTest()
    {
        bool result = researchSystem.StartResearch(TechID.CapacityUpgrade);

        Assert.IsFalse(result);
        Assert.IsNull(researchSystem.CurrentResearch);
    }

    [Test]
    public void StopResearchTest()
    {
        researchSystem.StartResearch(TechID.TrainSpeed);

        researchSystem.StopResearch();

        Assert.IsNull(researchSystem.CurrentResearch);
        Assert.IsFalse(researchSystem.GetTechnology(TechID.TrainSpeed).IsResearching);
    }

    [Test]
    public void AddResearchPointsTest()
    {
        researchSystem.StartResearch(TechID.TrainSpeed);

        researchSystem.AddResearchPoints(25);

        Assert.AreEqual(25f, researchSystem.GetTechnology(TechID.TrainSpeed).Progress);
    }

    [Test]
    public void AddResearchPointNoCurrentTechTest()
    {
        researchSystem.AddResearchPoints(25);

        Assert.AreEqual(0f, researchSystem.GetTechnology(TechID.TrainSpeed).Progress);
    }

    [Test]
    public void AddResearchUnlockTechnologyTest()
    {
        researchSystem.StartResearch(TechID.TrainSpeed);

        researchSystem.AddResearchPoints(100);

        Technology tech = researchSystem.GetTechnology(TechID.TrainSpeed);

        Assert.IsTrue(tech.IsUnlocked);
        Assert.IsNull(researchSystem.CurrentResearch);
    }

    [Test]
    public void UnlockTechnologyAllowsDependentTest()
    {
        researchSystem.StartResearch(TechID.TrainSpeed);
        researchSystem.AddResearchPoints(100);

        Assert.IsTrue(researchSystem.CanResearch(TechID.CapacityUpgrade));
    }

    [Test]
    public void NewResearchStopsPreviousResearchTest()
    {
        TechData secondBaseTech = CreateTech(TechID.RailMaintenance, 40);

        List<TechData> database = new List<TechData>
        {
            baseTech,
            dependentTech,
            secondBaseTech
        };

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", database);
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");

        researchSystem.StartResearch(TechID.TrainSpeed);
        researchSystem.StartResearch(TechID.RailMaintenance);

        Assert.IsFalse(researchSystem.GetTechnology(TechID.TrainSpeed).IsResearching);
        Assert.IsTrue(researchSystem.GetTechnology(TechID.RailMaintenance).IsResearching);

        Object.DestroyImmediate(secondBaseTech);
    }
}
