using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Economy/Economy Config")]
public class EconomyConfig : ScriptableObject
{
    public float EconomyTickIntervalDays = 1f;

    public float BaseConstructionCostPerCell = 10f;
    public float BaseLineMaintenanceFlat = 1f;
    public float BaseLineMaintenancePerCell = 0.25f;
    public float BaseTrainPurchaseCost = 50f;
    public float BaseTrainRepairCost = 10f;
    public float BaseTrainMaintenanceFlat = 2f;
    public float RefundRatio = 0.5f;

    public List<ResourceValueEntry> CargoValues = new();
    public List<TerrainModifierEntry> TerrainModifierEntries = new();
    public List<AttributeRelationRow> AttributeRelationRows = new();

    public TextAsset AttributeRelationCsv;
    public int AttributeMatrixStartRow = 2;
    public int AttributeMatrixStartColumn = 1;
    public float MissingAttributeMatrixValue = 5f;

     public TextAsset CargoValueCsv;
    public int CargoValueStartRow = 1;
    public int CargoValueStartColumn = 0;
    public float MissingCargoValue = 5f;

    public TextAsset TerrainModifierCsv;
    public int TerrainModifierConstructionRow = 2;
    public int TerrainModifierMaintenanceRow = 3;
    public int TerrainModifierStartColumn = 1;
    public float MissingConstructionModifier = 1f;
    public float MissingMaintenanceModifier = 1f;

    private void OnValidate()
    {
        SyncAttributeRelationMatrix();
        SyncCargoValues();
        SyncTerrainModiferEntries();
    }

    public float GetCargoValue(ResourceType resource)
    {
        foreach (var entry in CargoValues)
        {
            if (entry.Resource == resource)
                return entry.Value;
        }

        return MissingCargoValue;
    }

    public float GetConstructionModifier(TerrainType terrain)
    {
        foreach (var entry in TerrainModifierEntries)
        {
            if (entry.Terrain == terrain)
                return entry.ConstructionModifier;
        }

        return MissingConstructionModifier;
    }

    public float GetMaintenanceModifier(TerrainType terrain)
    {
        foreach (var entry in TerrainModifierEntries)
        {
            if (entry.Terrain == terrain)
                return entry.MaintenanceModifier;
        }

        return MissingMaintenanceModifier;
    }

    public float GetAttributeRelationValue(StationAttributeType from, StationAttributeType to)
    {
        foreach (var row in AttributeRelationRows)
        {
            if (row.Source != from)
                continue;

            foreach (var value in row.Values)
            {
                if (value.Target == to)
                    return value.Value;
            }

            break;
        }

        return MissingAttributeMatrixValue;
    }

    [ContextMenu("Import Attribute Relation Matrix .csv")]
    public void ImportAttributeRelationMatrixFromCsv()
    {
        SyncAttributeRelationMatrix();

        if (AttributeRelationCsv == null || string.IsNullOrWhiteSpace(AttributeRelationCsv.text))
        {
            Debug.LogWarning("AttributeRelationCsv is missing");
            return;
        }

        List<List<string>> table = ParseCsv(AttributeRelationCsv.text);
        StationAttributeType[] types =
            (StationAttributeType[])Enum.GetValues(typeof(StationAttributeType));

        for (int rowIndex = 0; rowIndex < types.Length; rowIndex++)
        {
            int csvRowIndex = AttributeMatrixStartRow + rowIndex;
            AttributeRelationRow row = AttributeRelationRows[rowIndex];
            List<string> csvRow = csvRowIndex < table.Count ? table[csvRowIndex] : null;

            for (int colIndex = 0; colIndex < types.Length; colIndex++)
            {
                int csvColIndex = AttributeMatrixStartColumn + colIndex;
                float value = ReadCellFloat(csvRow, csvColIndex, MissingAttributeMatrixValue);

                int valueIndex = row.Values.FindIndex(v => v.Target == types[colIndex]);
                if (valueIndex >= 0)
                {
                    AttributeRelationValue relationValue = row.Values[valueIndex];
                    relationValue.Value = value;
                    row.Values[valueIndex] = relationValue;
                }
            }

            AttributeRelationRows[rowIndex] = row;
        }

        Debug.Log("Attribute relation matrix  .csv imported");
    }

    [ContextMenu("Import Cargo Values  .csv")]
    public void ImportCargoValuesFromCsv()
    {
        SyncCargoValues();

        if (CargoValueCsv == null || string.IsNullOrWhiteSpace(CargoValueCsv.text))
        {
            Debug.LogWarning("CargoValueCsv missing");
            return;
        }

        List<List<string>> table = ParseCsv(CargoValueCsv.text);
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        List<string> csvRow = CargoValueStartRow < table.Count ? table[CargoValueStartRow] : null;

        for (int index = 0; index < resourceTypes.Length; index++)
        {
            int csvColIndex = CargoValueStartColumn + index;
            float value = ReadCellFloat(csvRow, csvColIndex, MissingCargoValue);

            int entryIndex = CargoValues.FindIndex(e => e.Resource == resourceTypes[index]);
            if (entryIndex >= 0)
            {
                ResourceValueEntry entry = CargoValues[entryIndex];
                entry.Value = value;
                CargoValues[entryIndex] = entry;
            }
        }

        Debug.Log("Cargo values .csv imported ");
    }

    [ContextMenu("Import Terrain Modifiers .csv")]
    public void ImportTerrainModifiersFromCsv()
    {
        SyncTerrainModiferEntries();

        if (TerrainModifierCsv == null || string.IsNullOrWhiteSpace(TerrainModifierCsv.text))
        {
            Debug.LogWarning("TerrainModifierCsv is missing");
            return;
        }

        List<List<string>> table = ParseCsv(TerrainModifierCsv.text);
        TerrainType[] terrainTypes = (TerrainType[])Enum.GetValues(typeof(TerrainType));

        List<string> constructionRow = TerrainModifierConstructionRow < table.Count ? table[TerrainModifierConstructionRow] : null;

        List<string> maintenanceRow = TerrainModifierMaintenanceRow < table.Count ? table[TerrainModifierMaintenanceRow] : null;

        for (int index = 0; index < terrainTypes.Length; index++)
        {
            int csvColIndex = TerrainModifierStartColumn + index;

            float constructionValue = ReadCellFloat(
                constructionRow,
                csvColIndex,
                MissingConstructionModifier
            );

            float maintenanceValue = ReadCellFloat(
                maintenanceRow,
                csvColIndex,
                MissingMaintenanceModifier
            );

            int entryIndex = TerrainModifierEntries.FindIndex(e => e.Terrain == terrainTypes[index]);
            if (entryIndex >= 0)
            {
                TerrainModifierEntry entry = TerrainModifierEntries[entryIndex];
                entry.ConstructionModifier = constructionValue;
                entry.MaintenanceModifier = maintenanceValue;
                TerrainModifierEntries[entryIndex] = entry;
            }
        }

        Debug.Log("Terrain modifier .csv imported");
    }

    private void SyncAttributeRelationMatrix()
    {
        StationAttributeType[] types =
            (StationAttributeType[])Enum.GetValues(typeof(StationAttributeType));

        foreach (StationAttributeType source in types)
        {
            int rowIndex = AttributeRelationRows.FindIndex(r => r.Source == source);
            AttributeRelationRow row;

            if (rowIndex >= 0)
            {
                row = AttributeRelationRows[rowIndex];
            }
            else
            {
                row = new AttributeRelationRow
                {
                    Source = source,
                    Values = new List<AttributeRelationValue>()
                };
            }

            if (row.Values == null)
                row.Values = new List<AttributeRelationValue>();

            foreach (StationAttributeType target in types)
            {
                int valueIndex = row.Values.FindIndex(v => v.Target == target);
                if (valueIndex < 0)
                {
                    row.Values.Add(new AttributeRelationValue
                    {
                        Target = target,
                        Value = MissingAttributeMatrixValue
                    });
                }
            }

            row.Values.RemoveAll(v => Array.IndexOf(types, v.Target) < 0);
            row.Values.Sort((a, b) => a.Target.CompareTo(b.Target));

            if (rowIndex >= 0)
                AttributeRelationRows[rowIndex] = row;
            else
                AttributeRelationRows.Add(row);
        }

        AttributeRelationRows.RemoveAll(r => Array.IndexOf(types, r.Source) < 0);
        AttributeRelationRows.Sort((a, b) => a.Source.CompareTo(b.Source));
    }

    private void SyncCargoValues()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        foreach (ResourceType resource in resourceTypes)
        {
            if (!CargoValues.Exists(e => e.Resource == resource))
            {
                CargoValues.Add(new ResourceValueEntry
                {
                    Resource = resource,
                    Value = MissingCargoValue
                });
            }
        }

        CargoValues.RemoveAll(e => Array.IndexOf(resourceTypes, e.Resource) < 0);
        CargoValues.Sort((a, b) => a.Resource.CompareTo(b.Resource));
    }

    private void SyncTerrainModiferEntries()
    {
        TerrainType[] terrainTypes = (TerrainType[])Enum.GetValues(typeof(TerrainType));

        foreach (TerrainType terrain in terrainTypes)
        {
            if (!TerrainModifierEntries.Exists(e => e.Terrain == terrain))
            {
                TerrainModifierEntries.Add(new TerrainModifierEntry
                {
                    Terrain = terrain,
                    ConstructionModifier = MissingConstructionModifier,
                    MaintenanceModifier = MissingMaintenanceModifier
                });
            }
        }

        TerrainModifierEntries.RemoveAll(e => Array.IndexOf(terrainTypes, e.Terrain) < 0);
        TerrainModifierEntries.Sort((a, b) => a.Terrain.CompareTo(b.Terrain));
    }

    private static float ReadCellFloat(List<string> row, int columnIndex, float fallback)
    {
        if (row == null || columnIndex < 0 || columnIndex >= row.Count)
            return fallback;

        string raw = row[columnIndex]?.Trim();
        if (string.IsNullOrEmpty(raw))
            return fallback;

        raw = raw.Replace(",", ".");

        return float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out float value) ? value : fallback;
    }

    private static List<List<string>> ParseCsv(string text)
    {
        List<List<string>> table = new();
        string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = normalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
            table.Add(ParseCsvLine(line));

        return table;
    }

    private static List<string> ParseCsvLine(string line)
    {
        List<string> result = new();
        if (string.IsNullOrEmpty(line))
        {
            result.Add(string.Empty);
            return result;
        }

        bool inQuotes = false;
        StringBuilder current = new();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}

[Serializable]
public struct ResourceValueEntry
{
    public ResourceType Resource;
    public float Value;
}

[Serializable]
public struct TerrainModifierEntry
{
    public TerrainType Terrain;
    public float ConstructionModifier;
    public float MaintenanceModifier;
}

[Serializable]
public struct AttributeRelationRow
{
    public StationAttributeType Source;
    public List<AttributeRelationValue> Values;
}

[Serializable]
public struct AttributeRelationValue
{
    public StationAttributeType Target;
    public float Value;
}
