using System;
using UnityEngine;

[CreateAssetMenu(menuName="Economy/TerrainConfig")]
public class TerrainConfig : ScriptableObject
{
    public float TerrainBaseCost = 100;
    private float[] rawTerrainCostMultipliers = new float[]
    {
        1f,     // Grassland - Равнина
        1.75f,  // Hills - Холмы
        1.5f,   // Forest - Лес
        1.25f,  // Desert - Пустыния
        1.75f,  // Tundra - Тундра
        1.75f,  // Swamp - Болото
        2.25f,  // Tropics - Тропики
        3f,     // River - Река
        4f      // Mountain - Горы
    };

    // Массив для лучшей читаемости в Unity
    public TerrainCostMultiplier[] TerrainCostMultipliers = new TerrainCostMultiplier[]
    {
        new TerrainCostMultiplier { TerrainType = TerrainType.Grassland, Multiplier = 1f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Hills, Multiplier = 1.75f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Forest, Multiplier = 1.5f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Desert, Multiplier = 1.25f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Tundra, Multiplier = 1.75f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Swamp, Multiplier = 1.75f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Tropics, Multiplier = 2.25f },
        new TerrainCostMultiplier { TerrainType = TerrainType.River, Multiplier = 3f },
        new TerrainCostMultiplier { TerrainType = TerrainType.Mountain, Multiplier = 4f }
    };
}

[Serializable]
public struct TerrainCostMultiplier
{
    public TerrainType TerrainType;
    public float Multiplier;
}