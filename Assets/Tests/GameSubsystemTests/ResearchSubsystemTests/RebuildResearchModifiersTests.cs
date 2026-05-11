using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RebuildResearchModifiersTests
{
    private GameObject researchGO;
    private GameObject modifierGO;
    private ResearchSystem researchSystem;
    private ResearchModifierSystem modifierSystem;
    private TechData laketech;
    private TechData speedTech;

    [SetUp]
    public void SetUp()
    {
        researchGO = new GameObject();
        researchSystem = researchGO.AddComponent<ResearchSystem>();

        modifierGO = new GameObject();
        modifierSystem = modifierGO.AddComponent<ResearchModifierSystem>();
        modifierSystem.ResetModifiers();

        laketech = CreateTech(TechID.FreshwaterBridge, 10);
        speedTech = CreateTech(TechID.TrainSpeed, 10);

        List<TechData> database = new List<TechData>
        {
            laketech,
            speedTech
        };

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", database);
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(researchGO);
        Object.DestroyImmediate(modifierGO);
        Object.DestroyImmediate(laketech);
        Object.DestroyImmediate(speedTech);
    }

    private TechData CreateTech(TechID ID, int cost)
    {
        TechData data = ScriptableObject.CreateInstance<TechData>();
        data.ID = ID;
        data.techName = ID.ToString();
        data.researchCost = cost;
        data.prerequisites = new List<TechID>();
        return data;
    }

    [Test]
    public void RebuildApplyTechnologyEffectsTest()
    {
        researchSystem.GetTechnology(TechID.FreshwaterBridge).Unlock();
        researchSystem.GetTechnology(TechID.TrainSpeed).Unlock();

        modifierSystem.RebuildFromResearch(researchSystem);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.AreEqual(0.85f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Lake));
        Assert.AreEqual(1.15f, modifierSystem.TrainSpeedResearchMultiplier);
    }

    [Test]
    public void RebuildIgnoredLockedTechnologiesTest()
    {
        modifierSystem.RebuildFromResearch(researchSystem);

        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.AreEqual(1f, modifierSystem.TrainSpeedResearchMultiplier);
    }
}
