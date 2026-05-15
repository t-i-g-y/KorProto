using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResearchModifierSystemPlayModeTests
{
    private GameObject researchGO;
    private ResearchSystem researchSystem;
    private GameObject modifierGO;
    private ResearchModifierSystem modifierSystem;
    private TechData techData;

    [SetUp]
    public void SetUp()
    {
        researchGO = new GameObject();
        researchSystem = researchGO.AddComponent<ResearchSystem>();

        modifierGO = new GameObject();
        modifierSystem = modifierGO.AddComponent<ResearchModifierSystem>();

        techData = ScriptableObject.CreateInstance<TechData>();
        techData.ID = TechID.AllSpeedUpgrade;
        techData.techName = "AllSpeedUpgrade";
        techData.researchCost = 10;
        techData.prerequisites = new List<TechID>();

        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", new List<TechData> { techData } );
        TestImmitationHelper.InvokePrivateMethod(researchSystem, "BuildRuntimeTechnologies");
    }

    [TearDown]
    public void TearDown()
    {
        if (modifierGO != null)
            Object.DestroyImmediate(modifierGO);

        if (researchGO != null)
            Object.DestroyImmediate(researchGO);

        if (techData != null)
            Object.DestroyImmediate(techData);
    }

    [UnityTest]
    public IEnumerator UnlockedTechnologyChangesRuntimeModifiers()
    {
        TestImmitationHelper.SetPrivateField(researchSystem, "techDatabase", new List<TechData> { techData } );
        yield return null;

        float before = modifierSystem.TrainSpeedResearchMultiplier;
        researchSystem.StartResearch(TechID.AllSpeedUpgrade);
        researchSystem.AddResearchPoints(10);
        modifierSystem.RebuildFromResearch(researchSystem);
        float after = modifierSystem.TrainSpeedResearchMultiplier;

        Assert.Greater(after, before);
    }
}
