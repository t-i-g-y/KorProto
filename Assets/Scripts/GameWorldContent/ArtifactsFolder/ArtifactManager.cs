using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ArtifactManager : MonoBehaviour
{
    [SerializeField] private List<ArtifactDefinition> artifactDefinitions = new();
    [SerializeField] private bool useBuiltInArtifactsWhenEmpty = true;
    [SerializeField] private Vector3 pickupOffset = new(0f, 0.2f, -0.1f);
    [SerializeField] private float pickupScale = 0.55f;
    [SerializeField] private int emptyTileSearchRadius = 8;

    private readonly List<ArtifactDefinition> runtimeDefinitions = new();
    private readonly List<ArtifactInventoryEntry> inventory = new();
    private readonly HashSet<string> acquiredArtifactIds = new();
    private readonly HashSet<string> spawnedArtifactIds = new();
    private readonly Dictionary<string, ArtifactPickup> activePickups = new();
    private readonly Dictionary<string, Sprite> fallbackSprites = new();

    private EventManager boundEventManager;
    private QuestManager boundQuestManager;
    private TimeManager boundTimeManager;
    private Grid cachedGrid;
    private Tilemap cachedLandTilemap;
    private Tilemap cachedWaterTilemap;

    public static ArtifactManager Instance { get; private set; }
    public IReadOnlyList<ArtifactInventoryEntry> Inventory => inventory;

    public event Action<ArtifactInventoryEntry> ArtifactCollected;
    public event Action<ArtifactPickup> ArtifactPickupSpawned;
    public event Action InventoryChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null || FindAnyObjectByType<ArtifactManager>() != null)
            return;

        GameObject managerObject = new("ArtifactManager");
        managerObject.AddComponent<ArtifactManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildRuntimeDefinitions();
    }

    private void OnEnable()
    {
        RailManager.LineCreated += HandleLineCreated;
        TrainManager.TrainCreated += HandleTrainCreated;
        Train.TrainBroken += HandleTrainBroken;
        TryBindManagers();
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        TrainManager.TrainCreated -= HandleTrainCreated;
        Train.TrainBroken -= HandleTrainBroken;
        UnbindManagers();
    }

    private void Update()
    {
        HandlePickupClick();
        TryBindManagers();
    }

    public void TriggerManualArtifact(ArtifactDefinition definition)
    {
        if (definition == null)
            return;

        TrySpawn(definition, BuildContext(ArtifactTriggerType.Manual));
    }

    public Sprite GetIcon(string artifactId)
    {
        ArtifactDefinition definition = FindDefinition(artifactId);
        return GetIcon(definition, artifactId);
    }

    public ArtifactManagerSaveData GetSaveData()
    {
        ArtifactManagerSaveData data = new();
        data.inventory.AddRange(inventory);

        foreach (string artifactId in spawnedArtifactIds)
            data.spawnedArtifactIds.Add(artifactId);

        foreach (ArtifactPickup pickup in activePickups.Values)
        {
            if (pickup == null)
                continue;

            data.activePickups.Add(new ArtifactPickupSaveData
            {
                artifactId = pickup.ArtifactId,
                cell = pickup.Cell
            });
        }

        return data;
    }

    public void LoadFromSaveData(ArtifactManagerSaveData data)
    {
        ClearActivePickups();
        inventory.Clear();
        acquiredArtifactIds.Clear();
        spawnedArtifactIds.Clear();

        if (data != null)
        {
            inventory.AddRange(data.inventory);

            foreach (ArtifactInventoryEntry entry in inventory)
            {
                if (!string.IsNullOrWhiteSpace(entry.artifactId))
                    acquiredArtifactIds.Add(entry.artifactId);
            }

            foreach (string artifactId in data.spawnedArtifactIds)
            {
                if (!string.IsNullOrWhiteSpace(artifactId))
                    spawnedArtifactIds.Add(artifactId);
            }

            foreach (ArtifactPickupSaveData pickupData in data.activePickups)
            {
                if (pickupData == null || string.IsNullOrWhiteSpace(pickupData.artifactId))
                    continue;

                if (!acquiredArtifactIds.Contains(pickupData.artifactId))
                    SpawnPickup(FindDefinition(pickupData.artifactId), pickupData.artifactId, pickupData.cell);
            }
        }

        InventoryChanged?.Invoke();
    }

    public void Collect(ArtifactPickup pickup)
    {
        if (pickup == null)
            return;

        string artifactId = pickup.ArtifactId;
        if (string.IsNullOrWhiteSpace(artifactId))
            return;

        activePickups.Remove(artifactId);

        if (acquiredArtifactIds.Contains(artifactId))
        {
            Destroy(pickup.gameObject);
            return;
        }

        ArtifactDefinition definition = FindDefinition(artifactId);
        ArtifactInventoryEntry entry = new()
        {
            artifactId = artifactId,
            title = definition != null ? definition.Title : artifactId,
            story = definition != null ? definition.Story : string.Empty,
            day = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0,
            hour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0
        };

        inventory.Add(entry);
        acquiredArtifactIds.Add(artifactId);

        Destroy(pickup.gameObject);

        ArtifactCollected?.Invoke(entry);
        InventoryChanged?.Invoke();
    }

    private void BuildRuntimeDefinitions()
    {
        runtimeDefinitions.Clear();

        if (!useBuiltInArtifactsWhenEmpty)
            return;

        runtimeDefinitions.Add(ArtifactDefinition.CreateRuntime(
            "builtin_builder_compass",
            "Старый компас строителя",
            "Потертый латунный компас нашли у свежей насыпи. Стрелка давно не указывает на север, но всякий раз замирает в сторону нового пути.",
            ArtifactTriggerType.RailLineCreated,
            0f,
            ArtifactSpawnMode.ContextRailLineEnd,
            false));

        runtimeDefinitions.Add(ArtifactDefinition.CreateRuntime(
            "builtin_dispatch_fragment",
            "Фрагмент диспетчерского расписания",
            "Обрывок расписания сохранил отметки первых рейсов. Между строками видны карандашные правки человека, который знал сеть лучше карты.",
            ArtifactTriggerType.DayReached,
            3f,
            ArtifactSpawnMode.RandomStation,
            false));

        runtimeDefinitions.Add(ArtifactDefinition.CreateRuntime(
            "builtin_engineer_seal",
            "Инженерная печать",
            "Тяжелая печать с гербом депо. Ею заверяли планы, когда сеть стала достаточно большой, чтобы о ней заговорили как о системе.",
            ArtifactTriggerType.QuestCompleted,
            0f,
            ArtifactSpawnMode.RandomRailLineCell,
            false,
            "builtin_build_5_rail_lines"));
    }

    private IEnumerable<ArtifactDefinition> GetActiveDefinitions()
    {
        bool hasConfiguredDefinitions = false;

        foreach (ArtifactDefinition definition in artifactDefinitions)
        {
            if (definition == null)
                continue;

            hasConfiguredDefinitions = true;
            yield return definition;
        }

        if (hasConfiguredDefinitions || !useBuiltInArtifactsWhenEmpty)
            yield break;

        foreach (ArtifactDefinition definition in runtimeDefinitions)
            yield return definition;
    }

    private void HandleDayChanged(int day)
    {
        EvaluateTrigger(ArtifactTriggerType.DayReached, BuildContext(ArtifactTriggerType.DayReached));
        EvaluateTrigger(ArtifactTriggerType.DayInterval, BuildContext(ArtifactTriggerType.DayInterval));
        EvaluateTrigger(ArtifactTriggerType.BalanceBelow, BuildContext(ArtifactTriggerType.BalanceBelow));
    }

    private void HandleLineCreated(RailLine line)
    {
        ArtifactWorldContext context = BuildContext(ArtifactTriggerType.RailLineCreated);
        context.RailLine = line;
        EvaluateTrigger(ArtifactTriggerType.RailLineCreated, context);

        context.TriggerType = ArtifactTriggerType.RailLineCountReached;
        EvaluateTrigger(ArtifactTriggerType.RailLineCountReached, context);
    }

    private void HandleTrainCreated(Train train, RailLine line)
    {
        ArtifactWorldContext context = BuildContext(ArtifactTriggerType.TrainCreated);
        context.Train = train;
        context.RailLine = line;
        EvaluateTrigger(ArtifactTriggerType.TrainCreated, context);

        context.TriggerType = ArtifactTriggerType.TrainCountReached;
        EvaluateTrigger(ArtifactTriggerType.TrainCountReached, context);
    }

    private void HandleTrainBroken(Train train)
    {
        ArtifactWorldContext context = BuildContext(ArtifactTriggerType.TrainBroken);
        context.Train = train;
        context.RailLine = train != null ? train.AssignedLine : null;
        EvaluateTrigger(ArtifactTriggerType.TrainBroken, context);
    }

    private void HandleEventResolved(GameEventRuntime runtime)
    {
        ArtifactWorldContext context = BuildContext(ArtifactTriggerType.EventResolved);
        context.EventRuntime = runtime;
        context.RailLine = runtime?.Context?.RailLine;
        context.Train = runtime?.Context?.Train;
        EvaluateTrigger(ArtifactTriggerType.EventResolved, context);
    }

    private void HandleQuestCompleted(QuestRuntime quest)
    {
        ArtifactWorldContext context = BuildContext(ArtifactTriggerType.QuestCompleted);
        context.Quest = quest;
        EvaluateTrigger(ArtifactTriggerType.QuestCompleted, context);
    }

    private void EvaluateTrigger(ArtifactTriggerType triggerType, ArtifactWorldContext context)
    {
        context.TriggerType = triggerType;

        foreach (ArtifactDefinition definition in GetActiveDefinitions())
        {
            if (definition == null || definition.TriggerType != triggerType)
                continue;

            TrySpawn(definition, context);
        }
    }

    private bool TrySpawn(ArtifactDefinition definition, ArtifactWorldContext context)
    {
        if (!CanSpawn(definition, context))
            return false;

        if (!TryResolveSpawnCell(definition, context, out Vector3Int cell))
        {
            Debug.LogWarning($"Не удалось найти пустой тайл для артефакта: {definition.Title}");
            return false;
        }

        if (!SpawnPickup(definition, definition.ArtifactId, cell))
            return false;

        spawnedArtifactIds.Add(definition.ArtifactId);
        Debug.Log($"Артефакт появился на карте: {definition.Title}");
        return true;
    }

    private bool CanSpawn(ArtifactDefinition definition, ArtifactWorldContext context)
    {
        string artifactId = definition.ArtifactId;

        if (!definition.CanRepeat)
        {
            if (acquiredArtifactIds.Contains(artifactId))
                return false;

            if (spawnedArtifactIds.Contains(artifactId))
                return false;
        }

        if (activePickups.ContainsKey(artifactId))
            return false;

        return definition.Matches(context);
    }

    private bool SpawnPickup(ArtifactDefinition definition, string artifactId, Vector3Int cell)
    {
        if (string.IsNullOrWhiteSpace(artifactId) || activePickups.ContainsKey(artifactId))
            return false;

        EnsureGrid();

        Vector3 worldPosition = cachedGrid != null
            ? cachedGrid.GetCellCenterWorld(cell) + pickupOffset
            : new Vector3(cell.x, cell.y, 0f) + pickupOffset;

        GameObject pickupObject = new($"ArtifactPickup_{artifactId}", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(ArtifactPickup));
        pickupObject.transform.position = worldPosition;
        pickupObject.transform.localScale = Vector3.one * pickupScale;
        DontDestroyOnLoad(pickupObject);

        ArtifactPickup pickup = pickupObject.GetComponent<ArtifactPickup>();
        pickup.Initialize(this, definition, cell, GetIcon(definition, artifactId));
        activePickups[artifactId] = pickup;

        ArtifactPickupSpawned?.Invoke(pickup);
        return true;
    }

    private bool TryResolveSpawnCell(ArtifactDefinition definition, ArtifactWorldContext context, out Vector3Int cell)
    {
        Vector3Int desiredCell = ResolveSpawnAnchorCell(definition, context);

        if (TryFindEmptyTileNear(desiredCell, out cell))
            return true;

        return TryFindRandomEmptyTile(out cell);
    }

    private Vector3Int ResolveSpawnAnchorCell(ArtifactDefinition definition, ArtifactWorldContext context)
    {
        return definition.SpawnMode switch
        {
            ArtifactSpawnMode.ContextRailLineEnd => ResolveContextRailLineCell(context),
            ArtifactSpawnMode.ContextTrainCell => ResolveContextTrainCell(context),
            ArtifactSpawnMode.RandomStation => ResolveRandomStationCell(),
            ArtifactSpawnMode.RandomRailLineCell => ResolveRandomRailLineCell(),
            ArtifactSpawnMode.FixedCell => definition.FixedCell,
            _ => Vector3Int.zero
        };
    }

    private static Vector3Int ResolveContextRailLineCell(ArtifactWorldContext context)
    {
        if (context?.RailLine != null)
            return UnityEngine.Random.value < 0.5f ? context.RailLine.Start : context.RailLine.End;

        return ResolveRandomRailLineCell();
    }

    private static Vector3Int ResolveContextTrainCell(ArtifactWorldContext context)
    {
        if (context?.Train != null && context.Train.AssignedLine != null)
            return context.Train.AssignedLine.Start;

        return ResolveContextRailLineCell(context);
    }

    private static Vector3Int ResolveRandomStationCell()
    {
        Station[] stations = FindObjectsByType<Station>(FindObjectsSortMode.None);
        if (stations != null && stations.Length > 0)
            return stations[UnityEngine.Random.Range(0, stations.Length)].Cell;

        return ResolveRandomRailLineCell();
    }

    private static Vector3Int ResolveRandomRailLineCell()
    {
        if (RailManager.Instance != null && RailManager.Instance.Lines.Count > 0)
        {
            RailLine line = RailManager.Instance.Lines[UnityEngine.Random.Range(0, RailManager.Instance.Lines.Count)];
            if (line != null && line.Cells.Count > 0)
                return line.Cells[UnityEngine.Random.Range(0, line.Cells.Count)];
        }

        return Vector3Int.zero;
    }

    private bool TryFindEmptyTileNear(Vector3Int origin, out Vector3Int cell)
    {
        cell = default;

        int radiusLimit = Mathf.Max(0, emptyTileSearchRadius);
        Queue<Vector3Int> queue = new();
        HashSet<Vector3Int> visited = new();

        queue.Enqueue(origin);
        visited.Add(origin);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (HexDistance(origin, current) > radiusLimit)
                continue;

            if (IsEmptySpawnTile(current))
            {
                cell = current;
                return true;
            }

            int directionOffset = Mathf.Abs(origin.x + origin.y) % 6;
            for (int i = 0; i < 6; i++)
            {
                Vector3Int next = HexCoords.Neighbour(current, (i + directionOffset) % 6);
                if (visited.Add(next))
                    queue.Enqueue(next);
            }
        }

        return false;
    }

    private bool TryFindRandomEmptyTile(out Vector3Int cell)
    {
        cell = default;
        EnsureTilemaps();

        if (cachedLandTilemap == null)
            return TryFindEmptyTileNear(Vector3Int.zero, out cell);

        BoundsInt bounds = cachedLandTilemap.cellBounds;
        int attempts = Mathf.Max(64, bounds.size.x * bounds.size.y);

        for (int i = 0; i < attempts; i++)
        {
            Vector3Int candidate = new(
                UnityEngine.Random.Range(bounds.xMin, bounds.xMax),
                UnityEngine.Random.Range(bounds.yMin, bounds.yMax),
                0);

            if (IsEmptySpawnTile(candidate))
            {
                cell = candidate;
                return true;
            }
        }

        foreach (Vector3Int candidate in bounds.allPositionsWithin)
        {
            Vector3Int normalizedCandidate = new(candidate.x, candidate.y, 0);
            if (IsEmptySpawnTile(normalizedCandidate))
            {
                cell = normalizedCandidate;
                return true;
            }
        }

        return false;
    }

    private bool IsEmptySpawnTile(Vector3Int cell)
    {
        if (!IsPlayableTile(cell))
            return false;

        if (StationRegistry.TryGet(cell, out _))
            return false;

        if (RailAnchorRegistry.Instance != null && RailAnchorRegistry.Instance.IsAnchorCell(cell))
            return false;

        if (RelayStopRegistry.Instance != null && RelayStopRegistry.Instance.IsRelayCell(cell))
            return false;

        if (RailManager.Instance != null && RailManager.Instance.GetLinesAtCell(cell).Count > 0)
            return false;

        foreach (ArtifactPickup pickup in activePickups.Values)
        {
            if (pickup != null && pickup.Cell == cell)
                return false;
        }

        return true;
    }

    private bool IsPlayableTile(Vector3Int cell)
    {
        EnsureTilemaps();

        if (cachedWaterTilemap != null && cachedWaterTilemap.HasTile(cell))
            return false;

        return cachedLandTilemap == null || cachedLandTilemap.HasTile(cell);
    }

    private static int HexDistance(Vector3Int a, Vector3Int b)
    {
        Vector2Int axialA = HexCoords.OffsetToAxial(a);
        Vector2Int axialB = HexCoords.OffsetToAxial(b);
        int dq = axialA.x - axialB.x;
        int dr = axialA.y - axialB.y;
        return (Mathf.Abs(dq) + Mathf.Abs(dq + dr) + Mathf.Abs(dr)) / 2;
    }

    private ArtifactWorldContext BuildContext(ArtifactTriggerType triggerType)
    {
        return new ArtifactWorldContext
        {
            TriggerType = triggerType,
            Day = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0,
            Hour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0,
            RailLineCount = RailManager.Instance != null ? RailManager.Instance.Lines.Count : 0,
            TrainCount = TrainManager.Instance != null ? TrainManager.Instance.Trains.Count : 0,
            Balance = FinanceSystem.Instance != null ? FinanceSystem.Instance.Balance : 0f
        };
    }

    private ArtifactDefinition FindDefinition(string artifactId)
    {
        foreach (ArtifactDefinition definition in GetActiveDefinitions())
        {
            if (definition != null && definition.ArtifactId == artifactId)
                return definition;
        }

        return null;
    }

    private Sprite GetIcon(ArtifactDefinition definition, string artifactId)
    {
        if (definition != null && definition.Icon != null)
            return definition.Icon;

        if (fallbackSprites.TryGetValue(artifactId, out Sprite sprite))
            return sprite;

        Color baseColor = Color.HSVToRGB(Mathf.Abs(artifactId.GetHashCode() % 360) / 360f, 0.58f, 0.9f);
        Texture2D texture = new(64, 64, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color border = new(0.08f, 0.08f, 0.08f, 1f);
        Color shine = Color.Lerp(baseColor, Color.white, 0.35f);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                bool edge = x < 4 || x >= texture.width - 4 || y < 4 || y >= texture.height - 4;
                bool diagonal = Mathf.Abs(x - y) < 4;
                texture.SetPixel(x, y, edge ? border : diagonal ? shine : baseColor);
            }
        }

        texture.Apply();
        sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 64f);
        fallbackSprites[artifactId] = sprite;
        return sprite;
    }

    private void EnsureGrid()
    {
        if (cachedGrid == null)
            cachedGrid = FindAnyObjectByType<Grid>();
    }

    private void EnsureTilemaps()
    {
        if (cachedLandTilemap != null)
            return;

        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap == null)
                continue;

            string tilemapName = tilemap.gameObject.name;
            if (cachedLandTilemap == null && tilemapName.Contains("Land", StringComparison.OrdinalIgnoreCase))
                cachedLandTilemap = tilemap;

            if (cachedWaterTilemap == null && tilemapName.Contains("Water", StringComparison.OrdinalIgnoreCase))
                cachedWaterTilemap = tilemap;
        }

        if (cachedLandTilemap == null && tilemaps.Length > 0)
            cachedLandTilemap = tilemaps[0];
    }

    private void TryBindManagers()
    {
        if (boundEventManager == null && EventManager.Instance != null)
        {
            boundEventManager = EventManager.Instance;
            boundEventManager.EventResolved += HandleEventResolved;
        }

        if (boundQuestManager == null && QuestManager.Instance != null)
        {
            boundQuestManager = QuestManager.Instance;
            boundQuestManager.QuestCompleted += HandleQuestCompleted;
        }

        if (boundTimeManager == null && TimeManager.Instance != null)
        {
            boundTimeManager = TimeManager.Instance;
            boundTimeManager.OnDayChanged += HandleDayChanged;
        }
    }

    private void UnbindManagers()
    {
        if (boundEventManager != null)
        {
            boundEventManager.EventResolved -= HandleEventResolved;
            boundEventManager = null;
        }

        if (boundQuestManager != null)
        {
            boundQuestManager.QuestCompleted -= HandleQuestCompleted;
            boundQuestManager = null;
        }

        if (boundTimeManager != null)
        {
            boundTimeManager.OnDayChanged -= HandleDayChanged;
            boundTimeManager = null;
        }
    }

    private void ClearActivePickups()
    {
        foreach (ArtifactPickup pickup in activePickups.Values)
        {
            if (pickup != null)
                Destroy(pickup.gameObject);
        }

        activePickups.Clear();
    }

    private void HandlePickupClick()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector2 screenPosition = mouse.position.ReadValue();
        Vector3 worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z));
        Vector2 point = new(worldPosition.x, worldPosition.y);

        Collider2D[] hits = Physics2D.OverlapPointAll(point);
        for (int i = 0; i < hits.Length; i++)
        {
            ArtifactPickup pickup = hits[i] != null ? hits[i].GetComponent<ArtifactPickup>() : null;
            if (pickup == null)
                continue;

            Collect(pickup);
            return;
        }
    }
}
