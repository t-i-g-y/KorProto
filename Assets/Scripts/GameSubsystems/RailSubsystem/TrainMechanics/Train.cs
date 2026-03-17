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

    [Header("Consist")]
    [SerializeField] private TrainConsist attachedTrainConsist;
    [SerializeField] private TrainWagonView wagonViewPrefab;
    [SerializeField] private List<TrainWagonView> attachedWagonViews = new();

    [Header("Config")]
    [SerializeField] private TrainConfig config;
    [SerializeField] private bool onlyLoadRequested = true;

    private float[] segmentLengths;
    private float totalRouteLength;
    private float headDistance;
    private bool isDwelling;
    private int speedLevel = 1;

    public RailLine AssignedLine => assignedLine;
    public bool IsOperational => isOperational;
    public TrainConsist AttachedTrainConsist => attachedTrainConsist;
    public int SpeedLevel => speedLevel;

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

    public void Initialize(RailLine line, int trainId, TrainConfig trainConfig)
    {
        assignedLine = line;
        id = trainId;
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
        if (!isOperational || isDwelling || routeCoords == null || routeCoords.Count < 2)
            return;

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

    public IEnumerator ArriveAtStation(Station station)
    {
        isDwelling = true;
        atStation = true;

        Vector3 popupBasePosition = transform.position + incomePopupOffset;
        int popupStackIndex = 0;

        if (assignedLine != null && RailEconomySystem.Instance != null)
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
                CargoSaleResult sale = attachedTrainConsist.TryUnloadOne(station);
                if (!sale.Sold)
                    break;

                RefreshCargoVisuals();

                IncomePopupSpawner.Instance?.QueueCargoSale(
                    transform,
                    popupBasePosition,
                    sale.Resource,
                    sale.Value,
                    popupStackIndex
                );

                popupStackIndex++;
                yield return new WaitForSeconds(config.TimePerUnloadSec / TimeManager.Instance.TimeMultiplier);
            }

            while (attachedTrainConsist.TryLoadOne(station, onlyLoadRequested))
            {
                RefreshCargoVisuals();
                yield return new WaitForSeconds(config.TimePerLoadSec / TimeManager.Instance.TimeMultiplier);
            }
        }

        ReverseAndResume();
    }

    public IEnumerator ArriveAtRelay(RelayStop relay)
    {
        isDwelling = true;
        atStation = true;

        yield return new WaitForSeconds(stationDwellSeconds);

        ReverseAndResume();
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
            spriteRenderer.color = isSelected ? selectedColor : normalColor;
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
}
