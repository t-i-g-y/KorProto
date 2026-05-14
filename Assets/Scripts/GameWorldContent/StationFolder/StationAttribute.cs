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
    OilIndustry,
    ScientificCenter,
    Institute,
    University,
    Hospital,
    PowerPlant,
    ChemicalPlant,
    ElectronicsFactory
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
                AddProduced(ResourceType.Workforce, 3);
                AddProduced(ResourceType.Money, 2);
                AddConsumed(ResourceType.Water, 2);
                AddConsumed(ResourceType.Food, 2);
                AddConsumed(ResourceType.Electricity, 1);
                AddConsumed(ResourceType.Goods, 1);
                break;

            case StationAttributeType.Village:
                AddProduced(ResourceType.Milk, 2);
                AddProduced(ResourceType.Food, 2);
                AddProduced(ResourceType.Workforce, 1);
                AddConsumed(ResourceType.Water, 2);
                AddConsumed(ResourceType.Tools, 1);
                break;

            case StationAttributeType.Factory:
                AddProduced(ResourceType.Goods, 3);
                AddConsumed(ResourceType.Metal, 2);
                AddConsumed(ResourceType.Plastic, 1);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.Port:
                AddProduced(ResourceType.Fish, 2);
                AddProduced(ResourceType.LogisticsService, 2);
                AddConsumed(ResourceType.Fuel, 1);
                AddConsumed(ResourceType.Goods, 1);
                break;

            case StationAttributeType.LogisticsCenter:
                AddProduced(ResourceType.LogisticsService, 3);
                AddConsumed(ResourceType.Fuel, 2);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.FinancialCenter:
                AddProduced(ResourceType.Money, 4);
                AddProduced(ResourceType.Investments, 2);
                AddConsumed(ResourceType.Electricity, 1);
                AddConsumed(ResourceType.Goods, 1);
                break;

            case StationAttributeType.TourismCenter:
                AddProduced(ResourceType.Money, 2);
                AddProduced(ResourceType.Culture, 2);
                AddConsumed(ResourceType.Food, 2);
                AddConsumed(ResourceType.Water, 1);
                AddConsumed(ResourceType.Goods, 1);
                break;

            case StationAttributeType.Seaport:
                AddProduced(ResourceType.Fish, 2);
                AddProduced(ResourceType.LogisticsService, 3);
                AddProduced(ResourceType.ImportedGoods, 2);
                AddConsumed(ResourceType.Fuel, 2);
                AddConsumed(ResourceType.Metal, 1);
                AddConsumed(ResourceType.Plastic, 2);
                break;

            case StationAttributeType.FurnitureFactory:
                AddProduced(ResourceType.Goods, 2);
                AddConsumed(ResourceType.Wood, 2);
                AddConsumed(ResourceType.Tools, 1);
                break;

            case StationAttributeType.FoodIndustry:
                AddProduced(ResourceType.Food, 3);
                AddConsumed(ResourceType.Millet, 2);
                AddConsumed(ResourceType.Water, 1);
                AddConsumed(ResourceType.Milk, 1);
                break;

            case StationAttributeType.TextileIndustry:
                AddProduced(ResourceType.Fabric, 3);
                AddProduced(ResourceType.Goods, 1);
                AddConsumed(ResourceType.Cotton, 2);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.MechanicalEngineering:
                AddProduced(ResourceType.Equipment, 2);
                AddProduced(ResourceType.Tools, 2);
                AddConsumed(ResourceType.Metal, 2);
                AddConsumed(ResourceType.Coal, 1);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.Shipbuilding:
                AddProduced(ResourceType.Equipment, 3);
                AddConsumed(ResourceType.Metal, 2);
                AddConsumed(ResourceType.Wood, 2);
                AddConsumed(ResourceType.Fuel, 1);
                break;

            case StationAttributeType.WheatField:
                AddProduced(ResourceType.Millet, 3);
                AddConsumed(ResourceType.Water, 1);
                AddConsumed(ResourceType.Fertilizer, 1);
                break;

            case StationAttributeType.CattleFarm:
                AddProduced(ResourceType.Milk, 3);
                AddProduced(ResourceType.Food, 1);
                AddConsumed(ResourceType.Water, 2);
                AddConsumed(ResourceType.Millet, 1);
                break;

            case StationAttributeType.RiverFishing:
                AddProduced(ResourceType.Fish, 2);
                AddConsumed(ResourceType.Tools, 1);
                break;

            case StationAttributeType.SeaFishing:
                AddProduced(ResourceType.Fish, 3);
                AddConsumed(ResourceType.Fuel, 1);
                break;

            case StationAttributeType.ForestBelt:
                AddProduced(ResourceType.Wood, 3);
                AddConsumed(ResourceType.Tools, 1);
                break;

            case StationAttributeType.IronOreIndustry:
                AddProduced(ResourceType.IronOre, 3);
                AddConsumed(ResourceType.Coal, 1);
                AddConsumed(ResourceType.Tools, 1);
                break;

            case StationAttributeType.NonFerrousMetallurgy:
                AddProduced(ResourceType.Metal, 3);
                AddConsumed(ResourceType.IronOre, 2);
                AddConsumed(ResourceType.Coal, 2);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.CoalIndustry:
                AddProduced(ResourceType.Coal, 3);
                AddConsumed(ResourceType.Tools, 1);
                AddConsumed(ResourceType.Workforce, 1);
                break;

            case StationAttributeType.OilIndustry:
                AddProduced(ResourceType.Oil, 3);
                AddProduced(ResourceType.Fuel, 1);
                AddConsumed(ResourceType.Tools, 1);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.ScientificCenter:
                AddProduced(ResourceType.ResearchData, 3);
                AddProduced(ResourceType.Technology, 1);
                AddConsumed(ResourceType.Electricity, 2);
                AddConsumed(ResourceType.Equipment, 1);
                AddConsumed(ResourceType.Specialists, 1);
                break;

            case StationAttributeType.Institute:
                AddProduced(ResourceType.Specialists, 2);
                AddProduced(ResourceType.ResearchData, 1);
                AddConsumed(ResourceType.Money, 2);
                AddConsumed(ResourceType.Electricity, 1);
                AddConsumed(ResourceType.Goods, 1);
                break;

            case StationAttributeType.University:
                AddProduced(ResourceType.Specialists, 3);
                AddProduced(ResourceType.Technology, 1);
                AddConsumed(ResourceType.Money, 2);
                AddConsumed(ResourceType.Food, 1);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.Hospital:
                AddProduced(ResourceType.Medicine, 2);
                AddConsumed(ResourceType.Water, 1);
                AddConsumed(ResourceType.Electricity, 1);
                AddConsumed(ResourceType.Equipment, 1);
                break;

            case StationAttributeType.PowerPlant:
                AddProduced(ResourceType.Electricity, 4);
                AddConsumed(ResourceType.Coal, 2);
                AddConsumed(ResourceType.Fuel, 1);
                break;

            case StationAttributeType.ChemicalPlant:
                AddProduced(ResourceType.Plastic, 3);
                AddProduced(ResourceType.Fertilizer, 2);
                AddConsumed(ResourceType.Oil, 2);
                AddConsumed(ResourceType.Water, 1);
                AddConsumed(ResourceType.Electricity, 1);
                break;

            case StationAttributeType.ElectronicsFactory:
                AddProduced(ResourceType.Equipment, 2);
                AddProduced(ResourceType.Technology, 1);
                AddConsumed(ResourceType.Metal, 1);
                AddConsumed(ResourceType.Plastic, 1);
                AddConsumed(ResourceType.Electricity, 2);
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
