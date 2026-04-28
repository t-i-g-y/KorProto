using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ResearchModifierSystemTests
{
    private GameObject modifierGO;
    private ResearchModifierSystem modifierSystem;

    [SetUp]
    public void SetUp()
    {
        modifierGO = new GameObject();
        modifierSystem = modifierGO.AddComponent<ResearchModifierSystem>();

        modifierSystem.ResetModifiers();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(modifierGO);
    }

    [Test]
    public void ResetModifiersTest()
    {
        Assert.AreEqual(1f, modifierSystem.RailMaintenanceResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.TrainMaintenanceResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.CargoSaleIncomeResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.GlobalResearchIncomeMultiplier);
        Assert.AreEqual(0, modifierSystem.CargoCapacityBonus);
        Assert.AreEqual(0, modifierSystem.WagonUpgradeTiers);
    }

    [Test]
    public void ResetModifiersDefaultBuildableTerrainsTest()
    {
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Grassland));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Forest));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Hills));

        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.Mountain));
        Assert.IsFalse(modifierSystem.CanBuildOn(TerrainType.Sea));
    }

    [Test]
    public void FreshwaterBridgeUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.FreshwaterBridge);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.AreEqual(0.85f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Lake));
    }

    [Test]
    public void MountainTunnelUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.MountainTunnel);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Mountain));
        Assert.AreEqual(0.85f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Mountain));
    }

    [Test]
    public void SeaTunnelUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.SeaTunnel);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Sea));
        Assert.AreEqual(0.8f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Sea));
    }

    [Test]
    public void RailMaintenanceUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.RailMaintenance);

        Assert.AreEqual(0.85f, modifierSystem.RailMaintenanceResearchMultiplier);
    }

    [Test]
    public void TrainSpeedUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.TrainSpeed);

        Assert.AreEqual(1.15f, modifierSystem.TrainSpeedResearchMultiplier);
    }

    [Test]
    public void CapacityUpgradeUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.CapacityUpgrade);

        Assert.AreEqual(1.2f, modifierSystem.CargoCapacityResearchMultiplier);
        Assert.AreEqual(1, modifierSystem.WagonUpgradeTiers);
    }

    [Test]
    public void LoadUnloadSpeedUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.LoadUnloadSpeed);

        Assert.AreEqual(1.25f, modifierSystem.CargoLoadSpeedResearchMultiplier);
        Assert.AreEqual(1.25f, modifierSystem.CargoUnloadSpeedResearchMultiplier);
    }

    [Test]
    public void GlobalResearchUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.GlobalResearch);

        Assert.AreEqual(1.25f, modifierSystem.GlobalResearchIncomeMultiplier);
    }

    [Test]
    public void LocalResearchUnlockModifierTest()
    {
        modifierSystem.ApplyTechnology(TechID.LocalResearch);

        Assert.AreEqual(1.25f, modifierSystem.LocalResearchIncomeMultiplier);
    }
}