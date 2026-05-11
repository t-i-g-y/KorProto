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
        Assert.AreEqual(1f, modifierSystem.TrainCostResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.TrainBreakChanceMultiplier);
        Assert.AreEqual(1f, modifierSystem.WagonCostResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.RelayMaintenanceResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.WagonMaintenanceResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.CargoLoadSpeedResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.CargoUnloadSpeedResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.CargoSaleIncomeResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.RailConnectionIncomeResearchMultiplier);
        Assert.AreEqual(1f, modifierSystem.GlobalResearchIncomeMultiplier);
        Assert.AreEqual(1f, modifierSystem.LocalResearchIncomeMultiplier);
        Assert.AreEqual(1f, modifierSystem.CargoCapacityResearchMultiplier);
        Assert.AreEqual(0, modifierSystem.LocomotiveCargoCapacityBonus);
        Assert.AreEqual(0, modifierSystem.WagonCargoCapacityBonus);
        Assert.AreEqual(1, modifierSystem.WagonUpgradeTiers);
        Assert.AreEqual(1, modifierSystem.SpeedUpgradeTiers);
        Assert.AreEqual(0, modifierSystem.RailLengthBonus);
        Assert.AreEqual(0, modifierSystem.TrainPerLineBonus);
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
    public void BaseTerrainModifiersTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseTerrainModifiers);

        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Forest));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Hills));

        Assert.AreEqual(0.95f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainRelayMaintenanceMultiplier(TerrainType.Grassland));
    }

    [Test]
    public void BridgeUnlockTest()
    {
        modifierSystem.ApplyTechnology(TechID.BridgeUnlock);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Swamp));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Lake));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.River));
    }

    [Test]
    public void TundraUnlockTest()
    {
        modifierSystem.ApplyTechnology(TechID.TundraUnlock);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Tundra));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Hills));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Forest));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Hills));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainRelayMaintenanceMultiplier(TerrainType.Forest));
    }

    [Test]
    public void TerrainMaintenanceTest()
    {
        modifierSystem.ApplyTechnology(TechID.TerrainMaintenance);

        Assert.AreEqual(0.9f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainRelayMaintenanceMultiplier(TerrainType.Grassland));
    }

    [Test]
    public void DesertTropicsUnlockTest()
    {
        modifierSystem.ApplyTechnology(TechID.DesertTropicsUnlock);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Desert));
        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Tropics));

        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainRelayMaintenanceMultiplier(TerrainType.Grassland));
    }

    [Test]
    public void TerrainConstructionTest()
    {
        modifierSystem.ApplyTechnology(TechID.TerrainConstruction);

        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Forest));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Swamp));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Lake));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.River));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Tundra));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Tropics));
    }

    [Test]
    public void MountainTunnelTest()
    {
        modifierSystem.ApplyTechnology(TechID.MountainTunnel);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Mountain));
    }

    [Test]
    public void SeaTunnelTest()
    {
        modifierSystem.ApplyTechnology(TechID.SeaTunnel);

        Assert.IsTrue(modifierSystem.CanBuildOn(TerrainType.Sea));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Swamp));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Lake));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.River));
    }

    [Test]
    public void TerrainMaintenanceConstructionTest()
    {
        modifierSystem.ApplyTechnology(TechID.TerrainMaintenanceConstruction);

        Assert.AreEqual(0.9f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.9f, modifierSystem.GetTerrainRelayMaintenanceMultiplier(TerrainType.Grassland));
    }

    [Test]
    public void BaseRailMaintenanceTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseRailMaintenance);

        Assert.AreEqual(0.9f, modifierSystem.RailMaintenanceResearchMultiplier);
    }

    [Test]
    public void RailConnectionIncomeTest()
    {
        modifierSystem.ApplyTechnology(TechID.RailConnectionIncome);

        Assert.AreEqual(1.2f, modifierSystem.RailConnectionIncomeResearchMultiplier);
    }

    [Test]
    public void BaseRelayMaintenanceTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseRelayMaintenance);

        Assert.AreEqual(0.9f, modifierSystem.RelayMaintenanceResearchMultiplier);
        Assert.AreEqual(2, modifierSystem.RailLengthBonus);
    }

    [Test]
    public void BaseRailAndRelayMaintenanceTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseRailAndRelayMaintenance);

        Assert.AreEqual(0.95f, modifierSystem.RailMaintenanceResearchMultiplier);
        Assert.AreEqual(0.95f, modifierSystem.RelayMaintenanceResearchMultiplier);
    }

    [Test]
    public void RailConnectionIncome2Test()
    {
        modifierSystem.ApplyTechnology(TechID.RailConnectionIncome2);

        Assert.AreEqual(1.15f, modifierSystem.RailConnectionIncomeResearchMultiplier);
        Assert.AreEqual(1.15f, modifierSystem.CargoSaleIncomeResearchMultiplier);
    }

    [Test]
    public void RailConnectionIncome3Test()
    {
        modifierSystem.ApplyTechnology(TechID.RailConnectionIncome3);

        Assert.AreEqual(1.25f, modifierSystem.RailConnectionIncomeResearchMultiplier);
        Assert.AreEqual(1.25f, modifierSystem.CargoSaleIncomeResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.RelayMaintenanceResearchMultiplier);
    }

    [Test]
    public void BaseRailMaintenanceAndIncomeTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseRailMaintenanceAndIncome);

        Assert.AreEqual(0.9f, modifierSystem.RailMaintenanceResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.RelayMaintenanceResearchMultiplier);
        Assert.AreEqual(1.15f, modifierSystem.RailConnectionIncomeResearchMultiplier);
        Assert.AreEqual(2, modifierSystem.RailLengthBonus);
    }

    [Test]
    public void BaseTrainWagonMaintenanceTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseTrainWagonMaintenance);

        Assert.AreEqual(0.95f, modifierSystem.TrainMaintenanceResearchMultiplier);
        Assert.AreEqual(0.95f, modifierSystem.WagonMaintenanceResearchMultiplier);
    }

    [Test]
    public void BaseTrainCostTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseTrainCost);

        Assert.AreEqual(0.9f, modifierSystem.TrainCostResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.WagonCostResearchMultiplier);
        Assert.AreEqual(1.1f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(1, modifierSystem.RailLengthBonus);
    }

    [Test]
    public void BaseTrainWagonMaintenance2Test()
    {
        modifierSystem.ApplyTechnology(TechID.BaseTrainWagonMaintenance2);

        Assert.AreEqual(0.95f, modifierSystem.TrainMaintenanceResearchMultiplier);
        Assert.AreEqual(0.95f, modifierSystem.WagonMaintenanceResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.CargoLoadSpeedResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.CargoUnloadSpeedResearchMultiplier);
    }

    [Test]
    public void UpgradeTiersTest()
    {
        modifierSystem.ApplyTechnology(TechID.UpgradeTiers);

        Assert.AreEqual(2, modifierSystem.WagonUpgradeTiers);
        Assert.AreEqual(2, modifierSystem.SpeedUpgradeTiers);
        Assert.AreEqual(0.95f, modifierSystem.TrainCostResearchMultiplier);
    }

    [Test]
    public void WagonCapacityTest()
    {
        modifierSystem.ApplyTechnology(TechID.WagonCapacity);

        Assert.AreEqual(2, modifierSystem.WagonCargoCapacityBonus);
        Assert.AreEqual(0.95f, modifierSystem.WagonCostResearchMultiplier);
    }

    [Test]
    public void AllSpeedUpgradeTest()
    {
        modifierSystem.ApplyTechnology(TechID.AllSpeedUpgrade);

        Assert.AreEqual(1.2f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(0.8f, modifierSystem.CargoLoadSpeedResearchMultiplier);
        Assert.AreEqual(0.8f, modifierSystem.CargoUnloadSpeedResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.RelayMaintenanceResearchMultiplier);
    }

    [Test]
    public void UpgradeTiers2Test()
    {
        modifierSystem.ApplyTechnology(TechID.UpgradeTiers2);

        Assert.AreEqual(3, modifierSystem.WagonUpgradeTiers);
        Assert.AreEqual(2, modifierSystem.SpeedUpgradeTiers);
        Assert.AreEqual(2, modifierSystem.LocomotiveCargoCapacityBonus);
    }

    [Test]
    public void WagonUpgradeTest()
    {
        modifierSystem.ApplyTechnology(TechID.WagonUpgrade);

        Assert.AreEqual(3, modifierSystem.WagonUpgradeTiers);
        Assert.AreEqual(2, modifierSystem.WagonCargoCapacityBonus);
        Assert.AreEqual(1, modifierSystem.RailLengthBonus);
    }

    [Test]
    public void BaseTrainWagonCostAndMaintenanceTest()
    {
        modifierSystem.ApplyTechnology(TechID.BaseTrainWagonCostAndMaintenance);

        Assert.AreEqual(0.9f, modifierSystem.TrainCostResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.WagonCostResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.TrainMaintenanceResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.WagonMaintenanceResearchMultiplier);
        Assert.AreEqual(1.15f, modifierSystem.TrainSpeedResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.CargoLoadSpeedResearchMultiplier);
        Assert.AreEqual(0.9f, modifierSystem.CargoUnloadSpeedResearchMultiplier);
    }

    [Test]
    public void GlobalResearchTest()
    {
        modifierSystem.ApplyTechnology(TechID.GlobalResearch);

        Assert.AreEqual(1.1f, modifierSystem.GlobalResearchIncomeMultiplier);
    }

    [Test]
    public void GlobalLocalResearchTest()
    {
        modifierSystem.ApplyTechnology(TechID.GlobalLocalResearch);

        Assert.AreEqual(1.15f, modifierSystem.GlobalResearchIncomeMultiplier);
        Assert.AreEqual(1.15f, modifierSystem.LocalResearchIncomeMultiplier);
        Assert.AreEqual(0.95f, modifierSystem.TrainCostResearchMultiplier);
    }

    [Test]
    public void LocalResearchTest()
    {
        modifierSystem.ApplyTechnology(TechID.LocalResearch);

        Assert.AreEqual(1.1f, modifierSystem.LocalResearchIncomeMultiplier);
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainConstructionMultiplier(TerrainType.Grassland));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainRailMaintenanceMultiplier(TerrainType.Forest));
        Assert.AreEqual(0.95f, modifierSystem.GetTerrainRelayMaintenanceMultiplier(TerrainType.Hills));
    }

    [Test]
    public void LocalResearch2Test()
    {
        modifierSystem.ApplyTechnology(TechID.LocalResearch2);

        Assert.AreEqual(1.25f, modifierSystem.LocalResearchIncomeMultiplier);
    }

    [Test]
    public void GlobalLocalResearch2Test()
    {
        modifierSystem.ApplyTechnology(TechID.GlobalLocalResearch2);

        Assert.AreEqual(1.5f, modifierSystem.GlobalResearchIncomeMultiplier);
        Assert.AreEqual(1.1f, modifierSystem.LocalResearchIncomeMultiplier);
    }
}