using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;

public class EconomyPerformanceTests
{
    [Test, Performance]
    public void ImportCargoValuesCSVPerformance()
    {
        EconomyConfig config = ScriptableObject.CreateInstance<EconomyConfig>();

        config.CargoValueStartRow = 1;
        config.CargoValueStartColumn = 0;
        config.CargoValueCsv = new TextAsset("a\n10,20,30,40,50,60,70,80,90,100");

        Measure.Method(() =>
        {
            config.ImportCargoValuesFromCsv();
        })
        .WarmupCount(5)
        .MeasurementCount(50)
        .Run();

        Object.DestroyImmediate(config);
    }

    [Test, Performance]
    public void ImportTerrainModifiersCSVPerformance()
    {
        EconomyConfig config = ScriptableObject.CreateInstance<EconomyConfig>();

        config.TerrainModifierConstructionRow = 1;
        config.TerrainModifierMaintenanceRow = 2;
        config.TerrainModifierStartColumn = 0;
        config.TerrainModifierCsv = new TextAsset("b\n1,1.2,1.4,1.6,1.8,2\n0.5,0.7,0.9,1.1,1.3,1.5");

        Measure.Method(() =>
        {
            config.ImportTerrainModifiersFromCsv();
        })
        .WarmupCount(5)
        .MeasurementCount(50)
        .Run();

        Object.DestroyImmediate(config);
    }

    [Test, Performance]
    public void ImportAttributeRelationMatrixCsv_Performance()
    {
        EconomyConfig config = ScriptableObject.CreateInstance<EconomyConfig>();

        StationAttributeType[] attributes = (StationAttributeType[])System.Enum.GetValues(typeof(StationAttributeType));

        config.AttributeMatrixStartRow = 1;
        config.AttributeMatrixStartColumn = 0;

        string csv = "с\n";

        for (int row = 0; row < attributes.Length; row++)
        {
            string[] cells = new string[attributes.Length];

            for (int col = 0; col < attributes.Length; col++)
                cells[col] = (row * 10 + col).ToString(System.Globalization.CultureInfo.InvariantCulture);

            csv += string.Join(",", cells);

            if (row < attributes.Length - 1)
                csv += "\n";
        }

        config.AttributeRelationCsv = new TextAsset(csv);

        Measure.Method(() =>
        {
            config.ImportAttributeRelationMatrixFromCsv();
        })
        .WarmupCount(5)
        .MeasurementCount(50)
        .Run();

        Object.DestroyImmediate(config);
    }
}
