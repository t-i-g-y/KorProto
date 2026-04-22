using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    [Header("ID")]
    [SerializeField] private int id;
    public int ID => id;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color brokenColor = Color.darkGray;
    [SerializeField] private CargoVisualizer cargoVisualizer;
    [SerializeField] private Vector3 incomePopupOffset = new Vector3(0f, 0.6f, 0f);

    [Header("Movement")]
    [SerializeField] private RailLine assignedLine;
    [SerializeField] private float speed = 0.6f;
    [SerializeField] private float arriveDistance = 0.02f;
    [SerializeField] private float stationDwellSeconds = 0.5f;
    [SerializeField] private List<Vector3Int> routeTiles = new();
    [SerializeField] private List<Vector3> routeCoords = new();
    [SerializeField] private int dir = 1;
    [SerializeField] private int currentTileIndex = 0;
    [SerializeField] private bool atStation = false;
    [SerializeField] private bool isOperational = true;
    [SerializeField] private bool isBroken = false;

    [Header("Consist")]
    [SerializeField] private TrainConsist attachedTrainConsist;
    [SerializeField] private TrainWagonView wagonViewPrefab;
    [SerializeField] private List<TrainWagonView> attachedWagonViews = new();

    [Header("Breaking")]
    [SerializeField] private float breakChancePerSecond = 0.002f;
    [SerializeField] private int repairCost = 10;

    [Header("Config")]
    [SerializeField] private TrainConfig config;
    [SerializeField] private bool onlyLoadRequested = true;

    private float[] segmentLengths;
    private float totalRouteLength;
    private float headDistance;
    private bool isDwelling;
    private int speedLevel = 1;
    private bool hasReportedBrokenState;

    public RailLine AssignedLine => assignedLine;
    public bool IsOperational => isOperational;
    public TrainConsist AttachedTrainConsist => attachedTrainConsist;
    public int SpeedLevel => speedLevel;
    public float Speed => speed;
    public bool IsBroken => isBroken;
    public int RepairCost => repairCost;

    public static event Action<Train> TrainBroken;
    public static event Action<Train> TrainRepaired;

    private void Awake()
    {
        if (attachedTrainConsist == null)
            attachedTrainConsist = GetComponent<TrainConsist>();
    }

    private void OnEnable()
    {
        RailManager.ActiveNetworkChanged += RefreshOperationalState;
    }

    private void OnDisable()
    {
        RailManager.ActiveNetworkChanged -= RefreshOperationalState;
    }

    private void Update()
    {
        HandleTrainMovement();
    }

    private void OnDestroy()
    {
        CleanupWagonViews();
    }

    private void CleanupWagonViews()
    {
        for (int i = 0; i < attachedWagonViews.Count; i++)
        {
            if (attachedWagonViews[i] != null)
                Destroy(attachedWagonViews[i].gameObject);
        }

        attachedWagonViews.Clear();
    }

    public void Initialize(RailLine line, int trainID, TrainConfig trainConfig)
    {
        assignedLine = line;
        id = trainID;
        config = trainConfig;
        RefreshOperationalState();
    }

    public void SetPath(List<Vector3Int> tiles, List<Vector3> coords)
    {
        routeTiles = tiles ?? new List<Vector3Int>();
        routeCoords = coords ?? new List<Vector3>();

        if (routeCoords.Count < 2)
        {
            Debug.LogError("train path must contain at least 2 tiles");
            enabled = false;
            return;
        }

        RebuildRouteCache();

        dir = 1;
        currentTileIndex = 0;
        atStation = false;
        isDwelling = false;
        headDistance = 0f;

        transform.position = routeCoords[0];
        EvaluatePoseAtDistance(0f, out _, out Vector3 forward);

        if (forward.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, forward);

        SyncWagonViews(true);
        RefreshCargoVisuals();
    }

    public void RefreshOperationalState()
    {
        isOperational = assignedLine != null && RailManager.Instance != null && RailManager.Instance.IsLineActive(assignedLine);
    }

    public void HandleTrainMovement()
    {
        if (!isOperational || isBroken || isDwelling || routeCoords == null || routeCoords.Count < 2)
            return;

        TryBreak();
        float delta = speed * TimeManager.Instance.CustomDeltaTime * dir;
        headDistance += delta;

        if (dir > 0 && headDistance >= totalRouteLength - arriveDistance)
        {
            headDistance = totalRouteLength;
            currentTileIndex = routeTiles.Count - 1;
            TryArriveAtEndpoint();
            return;
        }

        if (dir < 0 && headDistance <= arriveDistance)
        {
            headDistance = 0f;
            currentTileIndex = 0;
            TryArriveAtEndpoint();
            return;
        }

        EvaluatePoseAtDistance(headDistance, out Vector3 pos, out Vector3 forwardDir);
        transform.position = pos;

        if (forwardDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, forwardDir);

        UpdateCurrentTileIndexFromDistance();
        SyncWagonViews(false);
    }

    public IEnumerator ArriveAtStation(Station station, bool justCreated = false)
    {
        isDwelling = true;
        atStation = true;

        Vector3 popupBasePosition = transform.position + incomePopupOffset;
        int popupStackIndex = 0;

        if (!justCreated && assignedLine != null && RailEconomySystem.Instance != null)
        {
            float earnedIncome = RailEconomySystem.Instance.ApplyLineIncome(assignedLine);

            if (earnedIncome > 0f)
            {
                IncomePopupSpawner.Instance?.QueueRailIncome(transform, popupBasePosition, earnedIncome);
                popupStackIndex++;
            }
        }

        yield return new WaitForSeconds(stationDwellSeconds);

        if (attachedTrainConsist != null)
        {
            while (true)
            {
                CargoSaleResult sale = attachedTrainConsist.TryUnloadOneToStation(station);
                if (!sale.Sold)
                    break;

                RefreshCargoVisuals();

                IncomePopupSpawner.Instance?.QueueCargoSale(transform, popupBasePosition, sale.Resource, sale.Value,popupStackIndex);

                popupStackIndex++;
                yield return new WaitForSeconds(config.TimePerUnloadSec);
            }

            while (attachedTrainConsist.TryUnloadOneToStationTransit(station))
            {
                RefreshCargoVisuals();
                yield return new WaitForSeconds(config.TimePerUnloadSec);
            }

            if (GlobalDemandSystem.Instance != null && TryGetCurrentAndTwinnedCells(station.Cell, out Vector3Int twinnedCell))
            {
                int freeCapacity = attachedTrainConsist.totalCapacity - attachedTrainConsist.usedCapacity;

                foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
                {
                    while (freeCapacity > 0 && GlobalDemandSystem.Instance.GetStationTransitAmount(station.StationID, resource) > 0)
                    {
                        if (!GlobalDemandSystem.Instance.PeekStationTransitDestination(station.StationID, resource, out int destinationStationId))
                            break;

                        if (!StationRegistry.TryGet(destinationStationId, out Station destination) || destination == null)
                            break;

                        if (!RailManager.Instance.TryGetShortestPathFirstHop(station.Cell, destination.Cell, out Vector3Int firstHop, out float _))
                            break;

                        if (firstHop != twinnedCell)
                            break;

                        if (!attachedTrainConsist.TryLoadOneFromStationTransit(station, resource, destinationStationId))
                            break;

                        freeCapacity--;
                        RefreshCargoVisuals();
                        yield return new WaitForSeconds(config.TimePerLoadSec);
                    }
                    while (freeCapacity > 0 && station.GetSupplyAmount(resource) > 0)
                    {
                        if (!GlobalDemandSystem.Instance.TryGetBestDestinationForResource(station.Cell, twinnedCell, resource, out int destinationStationId))
                            break;

                        if (!attachedTrainConsist.TryLoadOneFromStation(station, resource, destinationStationId))
                            break;

                        freeCapacity--;
                        RefreshCargoVisuals();
                        yield return new WaitForSeconds(config.TimePerLoadSec);
                    }
                }
            }
        }

        if (justCreated)
        {
            atStation = false;
            isDwelling = false;
            SyncWagonViews(true);
        }
        else
        {
            ReverseAndResume();
        }
    }

    public IEnumerator ArriveAtRelay(RelayStop relay, bool justCreated = false)
    {
        isDwelling = true;
        atStation = true;

        yield return new WaitForSeconds(stationDwellSeconds);

        if (attachedTrainConsist != null && relay != null)
        {
            while (attachedTrainConsist.TryUnloadOneToRelay(relay))
            {
                RefreshCargoVisuals();
                yield return new WaitForSeconds(config.TimePerUnloadSec);
            }

            if (GlobalDemandSystem.Instance != null && TryGetCurrentAndTwinnedCells(relay.Cell, out Vector3Int twinnedCell))
            {
                int freeCapacity = attachedTrainConsist.totalCapacity - attachedTrainConsist.usedCapacity;

                foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
                {
                    while (freeCapacity > 0 && relay.GetAmount(resource) > 0)
                    {
                        if (!relay.PeekNextDestination(resource, out int destinationStationId))
                            break;

                        if (!StationRegistry.TryGet(destinationStationId, out Station destination) || destination == null)
                            break;

                        if (!RailManager.Instance.TryGetShortestPathFirstHop(relay.Cell, destination.Cell, out Vector3Int firstHop, out float _))
                            break;

                        if (firstHop != twinnedCell)
                            break;

                        if (!attachedTrainConsist.TryLoadOneFromRelay(relay, resource, destinationStationId))
                            break;

                        freeCapacity--;
                        RefreshCargoVisuals();
                        yield return new WaitForSeconds(config.TimePerLoadSec);
                    }
                }
            }
        }

        if (justCreated)
        {
            atStation = false;
            isDwelling = false;
            SyncWagonViews(true);
        }
        else
        {
            ReverseAndResume();
        }
    }

    public void ChangeSpeed(float multiplier)
    {
        speed = Mathf.Max(0f, speed * multiplier);
    }

    public void SetSpeedLevel(int level)
    {
        speedLevel = Mathf.Clamp(level, 1, 3);

        switch (speedLevel)
        {
            case 0: 
                speed = 0f; 
                break;
            case 1: 
                speed = 0.6f; 
                break;
            case 2: 
                speed = 0.8f; 
                break;
            case 3: 
                speed = 1.2f; 
                break;
        }
    }

    public void SyncWagonViews(bool snapImmediately = false)
    {
        if (attachedTrainConsist == null || wagonViewPrefab == null)
            return;

        int required = attachedTrainConsist.WagonCount;

        while (attachedWagonViews.Count < required)
        {
            TrainWagonView wagonView = Instantiate(wagonViewPrefab);
            wagonView.Initialize(this, attachedWagonViews.Count);
            attachedWagonViews.Add(wagonView);
        }

        while (attachedWagonViews.Count > required)
        {
            int last = attachedWagonViews.Count - 1;
            if (attachedWagonViews[last] != null)
                Destroy(attachedWagonViews[last].gameObject);

            attachedWagonViews.RemoveAt(last);
        }

        for (int i = 0; i < attachedWagonViews.Count; i++)
        {
            if (attachedWagonViews[i] != null)
                attachedWagonViews[i].HandleFollowMovement(snapImmediately);
        }

        RefreshCargoVisuals();
        SetSelectedVisual(TrainManager.Instance.SelectedTrain == this);
    }

    public bool TryAddWagon()
    {
        if (attachedTrainConsist == null)
            return false;

        bool added = attachedTrainConsist.TryAddWagon();
        if (!added)
            return false;

        SyncWagonViews(true);
        return true;
    }

    public void GetPoseAtDistanceBehindHead(float distanceBehindHead, out Vector3 position, out Vector3 forward)
    {
        float sampleDistance = headDistance - distanceBehindHead * dir;
        sampleDistance = Mathf.Clamp(sampleDistance, 0f, totalRouteLength);
        EvaluatePoseAtDistance(sampleDistance, out position, out forward);
    }

    public void SetSelectedVisual(bool isSelected)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = isSelected ? selectedColor : (isBroken ? brokenColor : normalColor);
        
        for (int i = 0; i < attachedWagonViews.Count; i++)
        {
            if (attachedWagonViews[i] == null)
                continue;

            attachedWagonViews[i].SetSelectedVisual(isSelected);
        }
    }

    public void TryHandleInitialEndpointLoad()
    {
        if (routeTiles == null || routeTiles.Count == 0)
            return;

        Vector3Int startCell = routeTiles[0];

        if (StationRegistry.TryGet(startCell, out Station station))
        {
            StartCoroutine(ArriveAtStation(station, true));
            return;
        }

        if (RelayStopRegistry.Instance != null && RelayStopRegistry.Instance.TryGet(startCell, out RelayStop relay))
        {
            StartCoroutine(ArriveAtRelay(relay, true));
        }
    }

    private void TryArriveAtEndpoint()
    {
        SyncWagonViews(false);

        if (routeTiles == null || routeTiles.Count == 0)
        {
            dir *= -1;
            return;
        }

        Vector3Int endpointCell = dir > 0 ? routeTiles[^1] : routeTiles[0];

        if (StationRegistry.TryGet(endpointCell, out Station station))
        {
            StartCoroutine(ArriveAtStation(station));
            return;
        }

        if (RelayStopRegistry.Instance != null && RelayStopRegistry.Instance.TryGet(endpointCell, out RelayStop relay))
        {
            StartCoroutine(ArriveAtRelay(relay));
            return;
        }

        dir *= -1;
    }

    private bool TryGetCurrentAndTwinnedCells(Vector3Int currentCell, out Vector3Int twinnedCell)
    {
        twinnedCell = default;

        if (assignedLine == null || RailManager.Instance == null)
            return false;

        return RailManager.Instance.TryGetTwinnedEndpoint(assignedLine, currentCell, out twinnedCell);
    }


    private void ReverseAndResume()
    {
        dir *= -1;

        EvaluatePoseAtDistance(headDistance, out Vector3 pos, out Vector3 forwardDir);
        transform.position = pos;

        if (forwardDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, forwardDir);

        SyncWagonViews(true);

        atStation = false;
        isDwelling = false;
    }

    private void RefreshCargoVisuals()
    {
        if (attachedTrainConsist == null)
            return;

        int headCapacity = attachedTrainConsist.GetHeadCapacity();

        if (cargoVisualizer != null)
            cargoVisualizer.VisualizeCargo(attachedTrainConsist.BuildCargoSlice(0, headCapacity));

        for (int i = 0; i < attachedWagonViews.Count; i++)
        {
            if (attachedWagonViews[i] == null || attachedWagonViews[i].CargoVisualizer == null)
                continue;

            int start = headCapacity;
            for (int w = 0; w < i; w++)
                start += attachedTrainConsist.GetUnitCapacity(w);

            int capacity = attachedTrainConsist.GetUnitCapacity(i);
            attachedWagonViews[i].CargoVisualizer.VisualizeCargo(attachedTrainConsist.BuildCargoSlice(start, capacity));
        }
    }

    private void RebuildRouteCache()
    {
        int segmentCount = Mathf.Max(0, routeCoords.Count - 1);
        segmentLengths = new float[segmentCount];
        totalRouteLength = 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            float length = Vector3.Distance(routeCoords[i], routeCoords[i + 1]);
            segmentLengths[i] = length;
            totalRouteLength += length;
        }
    }

    private void EvaluatePoseAtDistance(float distance, out Vector3 position, out Vector3 forward)
    {
        distance = Mathf.Clamp(distance, 0f, totalRouteLength);

        float walked = 0f;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            float segLength = segmentLengths[i];
            if (segLength <= 0.0001f)
                continue;

            if (walked + segLength >= distance)
            {
                float t = (distance - walked) / segLength;
                position = Vector3.Lerp(routeCoords[i], routeCoords[i + 1], t);

                Vector3 segmentForward = (routeCoords[i + 1] - routeCoords[i]).normalized;
                forward = dir >= 0 ? segmentForward : -segmentForward;
                return;
            }

            walked += segLength;
        }

        position = routeCoords[^1];
        Vector3 lastSegmentForward = (routeCoords[^1] - routeCoords[^2]).normalized;
        forward = dir >= 0 ? lastSegmentForward : -lastSegmentForward;
    }

    private void UpdateCurrentTileIndexFromDistance()
    {
        if (routeCoords == null || routeCoords.Count < 2)
            return;

        float walked = 0f;

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            float next = walked + segmentLengths[i];
            if (headDistance <= next)
            {
                currentTileIndex = Mathf.Clamp(i, 0, routeTiles.Count - 1);
                return;
            }

            walked = next;
        }

        currentTileIndex = routeTiles.Count - 1;
    }

    private void TryBreak()
    {
        if (isBroken || config == null || TimeManager.Instance == null)
            return;

        float chance = config.BreakChangePerSecond * TimeManager.Instance.CustomDeltaTime;

        if (UnityEngine.Random.value < chance)
        {
            isBroken = true;
            SetSelectedVisual(TrainManager.Instance != null && TrainManager.Instance.SelectedTrain == this);

            if (!hasReportedBrokenState)
            {
                hasReportedBrokenState = true;
                TrainBroken?.Invoke(this);
            }
        }
    }

    public void Repair()
    {
        if (!isBroken)
            return;

        isBroken = false;
        SetSelectedVisual(false);

        hasReportedBrokenState = false;
        SetSpeedLevel(speedLevel);
        TrainRepaired?.Invoke(this);
    }
    
    #region save subsytem
    public TrainSaveData GetSaveData()
    {
        return new TrainSaveData
        {
            ID = id,
            assignedLineID = assignedLine != null ? assignedLine.ID : -1,
            speedLevel = speedLevel,
            dir = dir,
            currentTileIndex = currentTileIndex,
            headDistance = headDistance,
            atStation = atStation,
            isOperational = isOperational,
            isBroken = isBroken,
            consist = attachedTrainConsist != null ? attachedTrainConsist.GetSaveData() : null
        };
    }

    public void LoadFromSaveData(TrainSaveData data)
    {
        if (data == null)
            return;

        id = data.ID;
        dir = data.dir;
        currentTileIndex = data.currentTileIndex;
        headDistance = data.headDistance;
        atStation = data.atStation;
        isOperational = data.isOperational;
        isBroken = data.isBroken;
        hasReportedBrokenState = isBroken;

        SetSpeedLevel(data.speedLevel);

        if (attachedTrainConsist != null && data.consist != null)
            attachedTrainConsist.LoadFromSaveData(data.consist);

        EvaluatePoseAtDistance(headDistance, out Vector3 pos, out Vector3 forwardDir);
        transform.position = pos;

        if (forwardDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, forwardDir);

        SyncWagonViews(true);
    }
    #endregion
}
