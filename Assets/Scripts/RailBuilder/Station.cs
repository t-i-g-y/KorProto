using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Station : MonoBehaviour 
{
    [Header("Grid")]
    public Vector3Int Cell;
    [SerializeField] private Grid grid;
    
    [Header("Enable production")]
    [SerializeField] private bool produceCircle, produceTriangle, produceSquare;

    [Header("Enable consumption")]
    [SerializeField] private bool consumeCircle, consumeTriangle, consumeSquare;

    [Header("State (Debug)")]
    public ResourceAmount[] supply = new ResourceAmount[]
    {
        new ResourceAmount(ResourceType.Circle),
        new ResourceAmount(ResourceType.Triangle),
        new ResourceAmount(ResourceType.Square)  
    };

    public ResourceAmount[] demand = new ResourceAmount[]
    {
        new ResourceAmount(ResourceType.Circle),
        new ResourceAmount(ResourceType.Triangle),
        new ResourceAmount(ResourceType.Square)
    };

    [Header("Spawn/Demand")]
    [SerializeField] private GameConfig config;
    private float spawnTimer, demandTimer;

    private void Awake()
    {
        if (grid)
            Cell = grid.WorldToCell(transform.position);
        foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
        {
            supply[(int)t].Amount = 0;
            demand[(int)t].Amount = 0;
        }
        StationRegistry.Register(this);
    }
    private void OnDestroy() => StationRegistry.Unregister(this);

    private void Update()
    {
        if (!config)
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

    private void TrySpawn()
    {
        int n = Random.Range(config.SpawnBatchMin, config.SpawnBatchMax + 1);
        void Add(ResourceType t)
        {
            if (supply[(int)t].Amount < config.StationSupplyCap)
                supply[(int)t] += n;
        }
        if (produceCircle)
            Add(ResourceType.Circle);
        if (produceTriangle)
            Add(ResourceType.Triangle);
        if (produceSquare)
            Add(ResourceType.Square);
    }

    private void TryRequest()
    {
        int n = Random.Range(config.SpawnBatchMin, config.SpawnBatchMax+1);
        void Add(ResourceType t)
        {
            demand[(int)t].Amount += n;
            GlobalDemand.Outstanding[(int)t] += n;
        }
        if (consumeCircle)
            Add(ResourceType.Circle);
        if (consumeTriangle)
            Add(ResourceType.Triangle);
        if (consumeSquare)
            Add(ResourceType.Square);
    }

    public bool Consumes(ResourceType t) =>
        (t == ResourceType.Circle && consumeCircle) ||
        (t == ResourceType.Triangle && consumeTriangle) ||
        (t == ResourceType.Square && consumeSquare);

    public bool Produces(ResourceType t) =>
        (t == ResourceType.Circle && produceCircle) ||
        (t == ResourceType.Triangle && produceTriangle) ||
        (t == ResourceType.Square && produceSquare);
}

