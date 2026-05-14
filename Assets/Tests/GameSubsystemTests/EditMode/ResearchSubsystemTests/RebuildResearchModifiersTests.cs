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
    private TechData bridgeTech;
    private TechData trainCostTech;

    [SetUp]
    public void SetUp()
    {
        researchGO = new GameObject();
        researchSystem = researchGO.AddComponent<ResearchSystem>();

        modifierGO = new GameObject();
        modifierSystem = modifierGO.AddComponent<ResearchModifierSystem>();
        modifierSystem.ResetModifiers();

        bridgeTech = CreateTech(TechID.BridgeUnlock, 10);
        trainCostTech = CreateTech(TechID.BaseTrainCost, 10);

        List<TechData> database = new List<TechData>
        {
            bridgeTech,
            trainCostTech
        };

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", database);
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(researchGO);
        Object.DestroyImmediate(modifierGO);
        Object.DestroyImmediate(bridgeTech);
        Object.DestroyImmediate(trainCostTech);
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
        researchSystem.GetTechnology(TechID.BridgeUnlock).Unlock();
        researchSystem.GetTechnology(TechID.BaseTrainCost).Unlock();

        modifierSystem.RebuildFromResearch(researchSystem);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.River));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Swamp));
        Assert.AreEqual(0.9f, modifierSystem.TrainCostResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.WagonCostResearchMultiplier);
        Assert.AreEqual(1.1f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(1, modifierSystem.RailLengthBonus);
    }

    [Test]
    public void RebuildIgnoredLockedTechnologiesTest()
    {
        modifierSystem.RebuildFromResearch(researchSystem);

        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.River));
        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.Swamp));
        Assert.AreEqual(1f, modifierSystem.TrainCostResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(0, modifierSystem.RailLengthBonus);
    }
}
