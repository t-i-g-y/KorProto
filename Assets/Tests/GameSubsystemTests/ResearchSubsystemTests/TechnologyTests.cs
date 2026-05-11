using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TechnologyTests
{
    private TechData CreateTech(int cost = 100)
    {
        TechData data = ScriptableObject.CreateInstance<TechData>();
        data.ID = TechID.TrainSpeed;
        data.techName = "Test Tech";
        data.researchCost = cost;
        return data;
    }

    [Test]
    public void TechnologyConstructorTest()
    {
        Technology tech = new Technology(CreateTech());

        Assert.AreEqual(0f, tech.Progress);
        Assert.IsFalse(tech.IsUnlocked);
        Assert.IsFalse(tech.IsResearching);
    }

    [Test]
    public void StartResearchStatusTest()
    {
        Technology tech = new Technology(CreateTech());

        tech.StartResearch();

        Assert.IsTrue(tech.IsResearching);
    }

    [Test]
    public void StopResearchStatusTest()
    {
        Technology tech = new Technology(CreateTech());

        tech.StartResearch();
        tech.StopResearch();

        Assert.IsFalse(tech.IsResearching);
    }

    [Test]
    public void AddProgressTest()
    {
        Technology tech = new Technology(CreateTech(100));

        tech.AddProgress(25);

        Assert.AreEqual(25f, tech.Progress);
    }

    [Test]
    public void AddNegativeProgressTest()
    {
        Technology tech = new Technology(CreateTech(100));

        tech.AddProgress(0);
        tech.AddProgress(-10);

        Assert.AreEqual(0f, tech.Progress);
    }

    [Test]
    public void AddProgressOverfillTest()
    {
        Technology tech = new Technology(CreateTech(100));

        tech.AddProgress(150);

        Assert.AreEqual(100f, tech.Progress);
        Assert.IsTrue(tech.IsComplete);
    }

    [Test]
    public void ProgressNormalizedTest()
    {
        Technology tech = new Technology(CreateTech(100));

        tech.AddProgress(25);

        Assert.AreEqual(0.25f, tech.ProgressNormalized);
    }

    [Test]
    public void ProgressNormalizedNoResearchTest()
    {
        Technology tech = new Technology(CreateTech(0));

        Assert.AreEqual(0f, tech.ProgressNormalized);
    }

    [Test]
    public void TechnologyUnlockTest()
    {
        Technology tech = new Technology(CreateTech(100));

        tech.StartResearch();
        tech.Unlock();

        Assert.IsTrue(tech.IsUnlocked);
        Assert.IsFalse(tech.IsResearching);
        Assert.AreEqual(100f, tech.Progress);
    }
}
