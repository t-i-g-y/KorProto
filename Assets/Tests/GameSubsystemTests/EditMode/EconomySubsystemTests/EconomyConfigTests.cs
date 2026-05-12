using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using System;
using NUnit.Framework;
using UnityEngine;

public class EconomyConfigTests
{
    private EconomyConfig config;

    [SetUp]
    public void SetUp()
    {
        config = ScriptableObject.CreateInstance<EconomyConfig>();

        TestImmitationHelper.InvokePrivateMethod(config, "OnValidate");
    }

    [Test]
    public void OnValidateResourceTypeTest()
    {
        ResourceType[] resources = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        Assert.AreEqual(resources.Length, config.CargoValues.Count);

        foreach (ResourceType resource in resources)
            Assert.IsTrue(config.CargoValues.Exists(e => e.Resource == resource));
    }

    [Test]
    public void OnValidateTerrainTypeTest()
    {
        TerrainType[] terrains = (TerrainType[])Enum.GetValues(typeof(TerrainType));

        Assert.AreEqual(terrains.Length, config.TerrainModifierEntries.Count);

        foreach (TerrainType terrain in terrains)
            Assert.IsTrue(config.TerrainModifierEntries.Exists(e => e.Terrain == terrain));
    }

    [Test]
    public void OnValidateStationAttributeRowsTest()
    {
        StationAttributeType[] attributes =
            (StationAttributeType[])Enum.GetValues(typeof(StationAttributeType));

        Assert.AreEqual(attributes.Length, config.AttributeRelationRows.Count);

        foreach (StationAttributeType source in attributes)
        {
            AttributeRelationRow row =
                config.AttributeRelationRows.Find(r => r.Source == source);

            Assert.AreEqual(attributes.Length, row.Values.Count);
        }
    }

    [Test]
    public void ResourceValueTest()
    {
        ResourceType resource = FirstResourceType();

        config.CargoValues.Clear();
        config.CargoValues.Add(new ResourceValueEntry
        {
            Resource = resource,
            Value = 42f
        });

        Assert.AreEqual(42f, config.GetCargoValue(resource));
    }

    [Test]
    public void MissingResourceValueTest()
    {
        config.CargoValues.Clear();
        config.MissingCargoValue = 9f;

        ResourceType resource = FirstResourceType();

        Assert.AreEqual(9f, config.GetCargoValue(resource));
    }


    [Test]
    public void TerrainModifiersTest()
    {
        TerrainType terrain = FirstTerrainType();

        config.TerrainModifierEntries.Clear();
        config.TerrainModifierEntries.Add(new TerrainModifierEntry
        {
            Terrain = terrain,
            ConstructionModifier = 1.7f,
            MaintenanceModifier = 0.8f
        });

        Assert.AreEqual(1.7f, config.GetConstructionModifier(terrain));
        Assert.AreEqual(0.8f, config.GetMaintenanceModifier(terrain));
    }

    [Test]
    public void ImportCargoValuesTest()
    {
        ResourceType[] resources = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        string header = BuildTestCSVRow(resources.Length, "CSV Test");
        string values = BuildTestNumberRow(resources.Length, 10f);

        config.CargoValueStartRow = 1;
        config.CargoValueStartColumn = 0;
        config.CargoValueCsv = new TextAsset(header + "\n" + values);

        config.ImportCargoValuesFromCsv();

        for (int i = 0; i < resources.Length; i++)
        {
            float expected = 10f + i;
            Assert.AreEqual(expected, config.GetCargoValue(resources[i]));
        }
    }

    [Test]
    public void ImportMissingCargoValuesTest()
    {
        ResourceType resource = FirstResourceType();

        config.MissingCargoValue = 99f;
        config.CargoValueStartRow = 1;
        config.CargoValueStartColumn = 0;
        config.CargoValueCsv = new TextAsset("Header\nnot-a-number");

        config.ImportCargoValuesFromCsv();

        Assert.AreEqual(99f, config.GetCargoValue(resource));
    }

    [Test]
    public void ImportTerrainModifiersTest()
    {
        TerrainType[] terrains = (TerrainType[])Enum.GetValues(typeof(TerrainType));

        config.TerrainModifierConstructionRow = 1;
        config.TerrainModifierMaintenanceRow = 2;
        config.TerrainModifierStartColumn = 0;

        string header = BuildTestCSVRow(terrains.Length, "CSV Test");
        string construction = BuildTestNumberRow(terrains.Length, 1f);
        string maintenance = BuildTestNumberRow(terrains.Length, 2f);

        config.TerrainModifierCsv = new TextAsset(header + "\n" + construction + "\n" + maintenance);

        config.ImportTerrainModifiersFromCsv();

        for (int i = 0; i < terrains.Length; i++)
        {
            Assert.AreEqual(1f + i, config.GetConstructionModifier(terrains[i]));
            Assert.AreEqual(2f + i, config.GetMaintenanceModifier(terrains[i]));
        }
    }

    [Test]
    public void ImportAttributeRelationMatrixTest()
    {
        StationAttributeType[] attributes = (StationAttributeType[])Enum.GetValues(typeof(StationAttributeType));

        config.AttributeMatrixStartRow = 1;
        config.AttributeMatrixStartColumn = 0;

        string csv = "Header\n";

        for (int row = 0; row < attributes.Length; row++)
        {
            csv += BuildTestNumberRow(attributes.Length, row * 10f);

            if (row < attributes.Length - 1)
                csv += "\n";
        }

        config.AttributeRelationCsv = new TextAsset(csv);

        config.ImportAttributeRelationMatrixFromCsv();

        for (int row = 0; row < attributes.Length; row++)
        {
            for (int col = 0; col < attributes.Length; col++)
            {
                float expected = row * 10f + col;
                Assert.AreEqual(expected, config.GetAttributeRelationValue(attributes[row], attributes[col]));
            }
        }
    }

    private static ResourceType FirstResourceType()
    {
        return (ResourceType)Enum.GetValues(typeof(ResourceType)).GetValue(0);
    }

    private static TerrainType FirstTerrainType()
    {
        return (TerrainType)Enum.GetValues(typeof(TerrainType)).GetValue(0);
    }

    private static string BuildTestCSVRow(int count, string value)
    {
        string[] cells = new string[count];
        for (int i = 0; i < count; i++)
            cells[i] = value;
        return string.Join(",", cells);
    }

    private static string BuildTestNumberRow(int count, float start)
    {
        string[] cells = new string[count];
        for (int i = 0; i < count; i++)
            cells[i] = (start + i).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return string.Join(",", cells);
    }
}
