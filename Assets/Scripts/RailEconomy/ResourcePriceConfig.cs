using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Economy/ResourcePriceConfig")]
public class ResourcePriceConfig : ScriptableObject
{
    public ResourcePrice[] ResourcePriceList =
    {
        new ResourcePrice { Resource = ResourceType.Circle, Price = 50 },
        new ResourcePrice { Resource = ResourceType.Triangle, Price = 80 },
        new ResourcePrice { Resource = ResourceType.Square, Price = 100 }
    };
}

[Serializable]
public struct ResourcePrice
{
    public ResourceType Resource;
    public float Price;
}