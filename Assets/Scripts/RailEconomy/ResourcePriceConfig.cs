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
        new ResourcePrice { Resource = ResourceType.Plastic, Price = 100 }
    };
}

[Serializable]
public struct ResourcePrice
{
    public ResourceType Resource;
    public float Price;
}