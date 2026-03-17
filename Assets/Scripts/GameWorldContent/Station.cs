using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Station : MonoBehaviour
{
    private int stationID;
    [Header("Grid / Tile")]
    [SerializeField] private Grid grid;
    [SerializeField] private Vector3Int cell;

    [Header("Station data")]
    [SerializeField] private List<StationAttribute> attributes = new();
    [SerializeField] private int population = 100;

    [Header("Resource lists")]
    [SerializeField]
    private List<ResourceAmount> producedResources = new();
    [SerializeField]
    private List<ResourceAmount> consumedResources = new();

    [Header("State")]
    [SerializeField] private ResourceAmount[] supply;
    [SerializeField] private ResourceAmount[] demand;

    [Header("Spawn / Demand")]
    [SerializeField] private GameConfig config;

    private float spawnTimer;
    private float demandTimer;

    public int StationID { get; set; }
    public Vector3Int Cell => cell;
    public int Population => population;
    public IReadOnlyList<StationAttribute> Attributes => attributes;
    public IReadOnlyList<ResourceAmount> ProducedResources => producedResources;
    public IReadOnlyList<ResourceAmount> ConsumedResources => consumedResources;
    public ResourceAmount[] Supply => supply;
    public ResourceAmount[] Demand => demand;

    private void Awake()
    {
        UpdateCellFromWorldPosition();
        InitializeResourceArrays();
        RebuildResourceListsFromAttributes();
        StationRegistry.Register(this);
    }

    private void OnDestroy()
    {
        StationRegistry.Unregister(this);
    }

    private void OnValidate()
    {
        InitializeResourceArrays();
        RebuildResourceListsFromAttributes();
    }

    private void Update()
    {
        if (config == null || TimeManager.Instance == null)
            return;

        spawnTimer += TimeManager.Instance.CustomDeltaTime;
        demandTimer += TimeManager.Instance.CustomDeltaTime;

        if (spawnTimer >= config.SpawnEverySec)
        {
            spawnTimer = 0f;
            TrySpawn();
        }

        if (demandTimer >= config.SpawnEverySec)
        {
            demandTimer = 0f;
            TryRequest();
        }
    }

    public void UpdateCellFromWorldPosition()
    {
        if (grid != null)
            cell = grid.WorldToCell(transform.position);
    }

    private void InitializeResourceArrays()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        if (supply == null || supply.Length != resourceTypes.Length)
            supply = new ResourceAmount[resourceTypes.Length];

        if (demand == null || demand.Length != resourceTypes.Length)
            demand = new ResourceAmount[resourceTypes.Length];

        for (int index = 0; index < resourceTypes.Length; index++)
        {
            if (supply[index].Type != resourceTypes[index])
                supply[index] = new ResourceAmount(resourceTypes[index], Mathf.Max(0, supply[index].Amount));

            if (demand[index].Type != resourceTypes[index])
                demand[index] = new ResourceAmount(resourceTypes[index], Mathf.Max(0, demand[index].Amount));
        }
    }

    public void RebuildResourceListsFromAttributes()
    {
        InitializeResourceArrays();

        producedResources ??= new List<ResourceAmount>();
        consumedResources ??= new List<ResourceAmount>();

        producedResources.Clear();
        consumedResources.Clear();

        if (attributes == null)
            return;

        foreach (StationAttribute attribute in attributes)
        {
            if (attribute == null)
                continue;

            attribute.Refresh();

            foreach (ResourceAmount resourceAmount in attribute.ProducedResources)
                AddOrIncreaseResource(producedResources, resourceAmount);

            foreach (ResourceAmount resourceAmount in attribute.ConsumedResources)
                AddOrIncreaseResource(consumedResources, resourceAmount);
        }
    }

    public bool Produces(ResourceType resourceType)
    {
        return IndexOfResource(producedResources, resourceType) >= 0;
    }

    public bool Consumes(ResourceType resourceType)
    {
        return IndexOfResource(consumedResources, resourceType) >= 0;
    }

    public void AddAttribute(StationAttribute attribute)
    {
        if (attribute == null)
            return;

        if (attributes.Contains(attribute))
            return;

        attributes.Add(attribute);
        RebuildResourceListsFromAttributes();
    }

    public void RemoveAttribute(StationAttribute attribute)
    {
        if (attribute == null)
            return;

        if (attributes.Remove(attribute))
            RebuildResourceListsFromAttributes();
    }

    public void SetPopulation(int newPopulation)
    {
        population = Mathf.Max(0, newPopulation);
    }

    public void IncreasePopulation(int amount)
    {
        if (amount <= 0)
            return;

        population += amount;
    }

    public void DecreasePopulation(int amount)
    {
        if (amount <= 0)
            return;

        population = Mathf.Max(0, population - amount);
    }

    private void TrySpawn()
    {
        if (producedResources.Count == 0)
            return;

        int amount = UnityEngine.Random.Range(config.SpawnBatchMin, config.SpawnBatchMax + 1);

        foreach (ResourceAmount producedResource in producedResources)
        {
            int index = (int)producedResource.Type;
            int spawnAmount = amount * producedResource.Amount;
            supply[index].Amount = Mathf.Min(supply[index].Amount + spawnAmount, config.StationSupplyCap);
        }
    }

    private void TryRequest()
    {
        if (consumedResources.Count == 0)
            return;

        int amount = UnityEngine.Random.Range(config.SpawnBatchMin, config.SpawnBatchMax + 1);

        foreach (ResourceAmount consumedResource in consumedResources)
        {
            int index = (int)consumedResource.Type;
            int demandAmount = amount * consumedResource.Amount;
            demand[index].Amount += demandAmount;
            GlobalDemandSystem.Instance.OutstandingTotals[index].Amount += demandAmount;
        }
    }

    public int GetProducedAmount(ResourceType resourceType)
    {
        int index = IndexOfResource(producedResources, resourceType);
        return index >= 0 ? producedResources[index].Amount : 0;
    }

    public int GetConsumedAmount(ResourceType resourceType)
    {
        int index = IndexOfResource(consumedResources, resourceType);
        return index >= 0 ? consumedResources[index].Amount : 0;
    }

    public int GetSupplyAmount(ResourceType resourceType)
    {
        return supply[(int)resourceType].Amount;
    }

    public int GetDemandAmount(ResourceType resourceType)
    {
        return demand[(int)resourceType].Amount;
    }

    public void AddSupply(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return;

        int index = (int)resourceType;
        int supplyCap = config != null ? config.StationSupplyCap : int.MaxValue;
        supply[index].Amount = Mathf.Min(supply[index].Amount + amount, supplyCap);
    }

    public void AddDemand(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return;

        int index = (int)resourceType;
        demand[index].Amount += amount;
        GlobalDemandSystem.Instance.OutstandingTotals[index].Amount += amount;
    }

    public bool TryTakeSupply(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return false;

        int index = (int)resourceType;
        if (supply[index].Amount < amount)
            return false;

        supply[index].Amount -= amount;
        return true;
    }

    public bool TrySatisfyDemand(ResourceType resourceType, int amount)
    {
        if (amount <= 0)
            return false;

        int index = (int)resourceType;
        if (demand[index].Amount <= 0)
            return false;

        int delivered = Mathf.Min(demand[index].Amount, amount);
        demand[index].Amount -= delivered;
        GlobalDemandSystem.Instance.OutstandingTotals[index].Amount = Mathf.Max(0, GlobalDemandSystem.Instance.OutstandingTotals[index].Amount - delivered);
        return delivered > 0;
    }

    private static void AddOrIncreaseResource(List<ResourceAmount> resources, ResourceAmount resourceAmount)
    {
        if (resourceAmount.Amount <= 0)
            return;

        int index = IndexOfResource(resources, resourceAmount.Type);
        if (index >= 0)
        {
            ResourceAmount current = resources[index];
            current.Amount += resourceAmount.Amount;
            resources[index] = current;
            return;
        }

        resources.Add(new ResourceAmount(resourceAmount.Type, resourceAmount.Amount));
    }

    private static int IndexOfResource(List<ResourceAmount> resources, ResourceType resourceType)
    {
        if (resources == null)
            return -1;

        for (int index = 0; index < resources.Count; index++)
        {
            if (resources[index].Type == resourceType)
                return index;
        }

        return -1;
    }
}