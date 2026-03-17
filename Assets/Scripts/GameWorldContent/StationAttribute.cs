using System;
using System.Collections.Generic;
using UnityEngine;

// Перечисление названий атрибутов станции
public enum StationAttributeType
{
    City,
    Village,
    Factory,
    Port,
    LogisticsCenter,
    FinancialCenter,
    TourismCenter,
    Seaport,
    FurnitureFactory,
    FoodIndustry,
    TextileIndustry,
    MechanicalEngineering,
    Shipbuilding,
    WheatField,
    CattleFarm,
    RiverFishing,
    SeaFishing,
    ForestBelt,
    IronOreIndustry,
    NonFerrousMetallurgy,
    CoalIndustry,
    OilIndustry
}

[Serializable]
public class StationAttribute
{
    [SerializeField] private StationAttributeType attributeType;
    [SerializeField] private List<ResourceAmount> producedResources = new();
    [SerializeField] private List<ResourceAmount> consumedResources = new();

    public StationAttributeType AttributeType => attributeType;
    public IReadOnlyList<ResourceAmount> ProducedResources => producedResources;
    public IReadOnlyList<ResourceAmount> ConsumedResources => consumedResources;

    public bool Produces(ResourceType resourceType) => IndexOf(producedResources, resourceType) >= 0;
    public bool Consumes(ResourceType resourceType) => IndexOf(consumedResources, resourceType) >= 0;
    public int ProducedAmount(ResourceType resourceType) => AmountOf(producedResources, resourceType);
    public int ConsumedAmount(ResourceType resourceType) => AmountOf(consumedResources, resourceType);

    public void SetAttributeType(StationAttributeType type)
    {
        attributeType = type;
        FillResourcesByType();
    }

    public void Refresh()
    {
        FillResourcesByType();
    }

    private void FillResourcesByType()
    {
        producedResources.Clear();
        consumedResources.Clear();

        switch (attributeType)
        {
            case StationAttributeType.City:
                AddProduced(ResourceType.Water, 2);
                AddConsumed(ResourceType.Milk, 2);
                break;

            case StationAttributeType.Village:
                AddProduced(ResourceType.Milk, 2);
                AddConsumed(ResourceType.Water, 2);
                break;

            case StationAttributeType.Factory:
                AddProduced(ResourceType.Plastic, 3);
                AddConsumed(ResourceType.Coal, 2);
                AddConsumed(ResourceType.Iron, 2);
                break;

            case StationAttributeType.Port:
                AddProduced(ResourceType.Water, 2);
                AddConsumed(ResourceType.Plastic, 1);
                break;

            case StationAttributeType.LogisticsCenter:
                AddProduced(ResourceType.Water, 1);
                AddConsumed(ResourceType.Coal, 2);
                AddConsumed(ResourceType.Iron, 2);
                break;

            case StationAttributeType.FinancialCenter:
                AddProduced(ResourceType.Water, 1);
                AddConsumed(ResourceType.Plastic, 2);
                break;

            case StationAttributeType.TourismCenter:
                AddProduced(ResourceType.Water, 1);
                AddConsumed(ResourceType.Milk, 2);
                AddConsumed(ResourceType.Millet, 2);
                break;

            case StationAttributeType.Seaport:
                AddProduced(ResourceType.Water, 2);
                AddConsumed(ResourceType.Plastic, 2);
                AddConsumed(ResourceType.Coal, 1);
                break;

            case StationAttributeType.FurnitureFactory:
                AddProduced(ResourceType.Plastic, 2);
                AddConsumed(ResourceType.Iron, 2);
                break;

            case StationAttributeType.FoodIndustry:
                AddProduced(ResourceType.Milk, 3);
                AddConsumed(ResourceType.Millet, 2);
                AddConsumed(ResourceType.Water, 1);
                break;

            case StationAttributeType.TextileIndustry:
                AddProduced(ResourceType.Plastic, 2);
                AddConsumed(ResourceType.Milk, 2);
                break;

            case StationAttributeType.MechanicalEngineering:
                AddProduced(ResourceType.Iron, 3);
                AddConsumed(ResourceType.Coal, 2);
                AddConsumed(ResourceType.Plastic, 1);
                break;

            case StationAttributeType.Shipbuilding:
                AddProduced(ResourceType.Iron, 3);
                AddConsumed(ResourceType.Coal, 2);
                AddConsumed(ResourceType.Plastic, 2);
                break;

            case StationAttributeType.WheatField:
                AddProduced(ResourceType.Millet, 3);
                break;

            case StationAttributeType.CattleFarm:
                AddProduced(ResourceType.Milk, 3);
                AddConsumed(ResourceType.Water, 2);
                break;

            case StationAttributeType.RiverFishing:
            case StationAttributeType.SeaFishing:
            case StationAttributeType.ForestBelt:
                AddProduced(ResourceType.Water, 2);
                break;

            case StationAttributeType.IronOreIndustry:
                AddProduced(ResourceType.Iron, 3);
                AddConsumed(ResourceType.Coal, 1);
                break;

            case StationAttributeType.NonFerrousMetallurgy:
                AddProduced(ResourceType.Iron, 2);
                AddConsumed(ResourceType.Coal, 2);
                break;

            case StationAttributeType.CoalIndustry:
                AddProduced(ResourceType.Coal, 3);
                break;

            case StationAttributeType.OilIndustry:
                AddProduced(ResourceType.Plastic, 3);
                AddConsumed(ResourceType.Coal, 2);
                break;
        }
    }

    private void AddProduced(ResourceType resourceType, int amount)
    {
        AddResource(producedResources, resourceType, amount);
    }

    private void AddConsumed(ResourceType resourceType, int amount)
    {
        AddResource(consumedResources, resourceType, amount);
    }

    private static void AddResource(List<ResourceAmount> resources, ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return;

        int index = IndexOf(resources, resourceType);
        if (index >= 0)
        {
            ResourceAmount resourceAmount = resources[index];
            resourceAmount.Amount += amount;
            resources[index] = resourceAmount;
            return;
        }

        resources.Add(new ResourceAmount(resourceType, amount));
    }

    private static int IndexOf(List<ResourceAmount> resources, ResourceType resourceType)
    {
        for (int index = 0; index < resources.Count; index++)
        {
            if (resources[index].Type == resourceType)
                return index;
        }

        return -1;
    }

    private static int AmountOf(List<ResourceAmount> resources, ResourceType resourceType)
    {
        int index = IndexOf(resources, resourceType);
        return index >= 0 ? resources[index].Amount : 0;
    }
}