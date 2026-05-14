using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Economy/ResourcePriceConfig")]
public class ResourcePriceConfig : ScriptableObject
{
    public ResourcePrice[] ResourcePriceList =
    {
        new ResourcePrice { Resource = ResourceType.Coal, Price = 50 },
        new ResourcePrice { Resource = ResourceType.Iron, Price = 80 },
        new ResourcePrice { Resource = ResourceType.Milk, Price = 65 },
        new ResourcePrice { Resource = ResourceType.Water, Price = 30 },
        new ResourcePrice { Resource = ResourceType.Millet, Price = 55 },
        new ResourcePrice { Resource = ResourceType.Plastic, Price = 100 },
        new ResourcePrice { Resource = ResourceType.Food, Price = 70 },
        new ResourcePrice { Resource = ResourceType.Fish, Price = 75 },
        new ResourcePrice { Resource = ResourceType.IronOre, Price = 60 },
        new ResourcePrice { Resource = ResourceType.Metal, Price = 120 },
        new ResourcePrice { Resource = ResourceType.Oil, Price = 110 },
        new ResourcePrice { Resource = ResourceType.Fuel, Price = 130 },
        new ResourcePrice { Resource = ResourceType.Wood, Price = 45 },
        new ResourcePrice { Resource = ResourceType.Electricity, Price = 90 },
        new ResourcePrice { Resource = ResourceType.Tools, Price = 95 },
        new ResourcePrice { Resource = ResourceType.Equipment, Price = 160 },
        new ResourcePrice { Resource = ResourceType.Goods, Price = 140 },
        new ResourcePrice { Resource = ResourceType.Money, Price = 1 },
        new ResourcePrice { Resource = ResourceType.Workforce, Price = 85 },
        new ResourcePrice { Resource = ResourceType.Specialists, Price = 170 },
        new ResourcePrice { Resource = ResourceType.ResearchData, Price = 190 },
        new ResourcePrice { Resource = ResourceType.Technology, Price = 240 },
        new ResourcePrice { Resource = ResourceType.Medicine, Price = 150 },
        new ResourcePrice { Resource = ResourceType.Fertilizer, Price = 80 },
        new ResourcePrice { Resource = ResourceType.Fabric, Price = 100 },
        new ResourcePrice { Resource = ResourceType.Cotton, Price = 65 },
        new ResourcePrice { Resource = ResourceType.Culture, Price = 120 },
        new ResourcePrice { Resource = ResourceType.Investments, Price = 200 },
        new ResourcePrice { Resource = ResourceType.LogisticsService, Price = 115 },
        new ResourcePrice { Resource = ResourceType.ImportedGoods, Price = 180 }
    };
}

[Serializable]
public struct ResourcePrice
{
    public ResourceType Resource;
    public float Price;
}
