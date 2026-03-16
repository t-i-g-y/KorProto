using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Train : MonoBehaviour
{
    // Заголовок - Движение поезда
    [Header("Motion")]
    [SerializeField] private float speedUnitsPerSec = 0.6f;
    private int speedLevel = 1;

    // Расстояние от центра конечно тайла, на котором поезд разворачивается
    [SerializeField] private float arriveSnap = 0.02f;

    // Скорость поворота
    [SerializeField] private float rotationSpeed = 10f;
    private Quaternion targetRotation;
    private Vector3 lastPosition;

    // Заголовок - Вместимость поезда
    [Header("Capacity")]
    [SerializeField] private int trainHeadCapacity = 6;
    [SerializeField] private int totalCapacity = 6;
    public bool onlyLoadRequested = false;

    // Заголовок - Временные характеристики
    [Header("Timing")]
    public TrainConfig config;

    // Заголовок - Экономические характеристики
    [Header("Economy")]
    // Стоимость обслуживания поезда
    [SerializeField] private float maintenanceCost = 10f;

    public RailLine AssignedLine;
    // Маршрут поезда в системе координат Unity
    [SerializeField] private List<Vector3> worldPts;
    // Маршрута поезда в системе координат тайлов (целочисленные координаты)
    [SerializeField] private List<Vector3Int> cells;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private CargoVisualizer visualizer;

    [Header("Wagons")]
    [SerializeField] private TrainWagon wagonPrefab;
    [SerializeField] private int maxWagons = 2;
    private List<TrainWagon> wagons = new();
    private List<CargoVisualizer> wagonVisualizers = new();
    private List<Vector3> positionHistory = new();

    // Индекс текущего тайла
    private int idx = 0;
    // Индекс направления поезда. 1 = от первого в списке до последнего, -1 = наоборот
    private int dir = 1;
    // Статус остановки поезда. True = поезд стоит на станции, False = поезд не задерживается на станции (или в движении)
    private bool dwelling = false;

    [SerializeField] private ResourceAmount[] cargo;

    private int CargoCount()
    {
        int total = 0;

        if (cargo == null)
            return 0;

        foreach (ResourceAmount resourceAmount in cargo)
            total += resourceAmount.Amount;

        return total;
    }

    private void Awake()
    {
        EnsureCargoInitialized();
    }

    public int ID { get; private set; }
    public int TotalCapacity => totalCapacity;
    public List<TrainWagon> Wagons => wagons;
    public float Speed => speedUnitsPerSec;
    public int SpeedLevel
    {
        get => speedLevel;
        set
        {
            if (value > 3 || value < 1)
                return;
            
            speedLevel = value;
        }

    }
    public void SetPath(List<Vector3> ptsWorld, List<Vector3Int> ptsCells)
    {
        EnsureCargoInitialized();
        worldPts = ptsWorld;
        cells = ptsCells;
        ID = TrainManager.Instance.NextID;
        if (worldPts == null || worldPts.Count < 2)
        {
            Destroy(gameObject);
            return;
        }
        idx = 0;
        dir = 1;
        transform.position = worldPts[0];
        lastPosition = transform.position;
        

        Vector3 initialDir = (worldPts[1] - worldPts[0]).normalized;
        if (initialDir.sqrMagnitude > 0.0001f)
            targetRotation = Quaternion.LookRotation(Vector3.forward, initialDir);
        else
            targetRotation = transform.rotation;
        transform.rotation = targetRotation;
    }


    private void Update()
    {
        HandleTrainMovement();
    }

    // Функция движения поезда meow
    private void HandleTrainMovement()
    {
        if (dwelling || worldPts == null || worldPts.Count < 2)
            return;

        var target = worldPts[idx + dir];
        var step = speedUnitsPerSec * TimeManager.Instance.CustomDeltaTime;

        Vector3 beforeMove = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, step);
        positionHistory.Insert(0, transform.position);
        int maxHistory = 64;
        if (positionHistory.Count > maxHistory)
            positionHistory.RemoveAt(positionHistory.Count - 1);
        Vector3 moveDir = (transform.position - beforeMove).normalized;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            var requiredRotation = Quaternion.LookRotation(Vector3.forward, moveDir);
            targetRotation = requiredRotation;
        }   
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * TimeManager.Instance.CustomDeltaTime);

        

        float snap = ((target == worldPts[0]) || (target == worldPts[^1])) ? arriveSnap : 0f;
        if (Vector3.Distance(transform.position, target) <= snap)
        {
            idx += dir;

            if ((idx == 0 || idx == cells.Count - 1) & StationRegistry.TryGet(cells[idx], out var station))
            {
                StartCoroutine(DwellAtStation(station));
                return;
            }

            if (idx == worldPts.Count - 1 || idx == 0)
                dir = -dir;
        }
    }

    // Функция задержки поезда на станции
    private IEnumerator DwellAtStation(Station station)
    {
        dwelling = true;
        yield return new WaitForSeconds(0.5f);
        FinanceManager.Instance.GenerateIncomeForRailLine(AssignedLine);
        FinanceManager.Instance.DeductMaintenanceCost(maintenanceCost);

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            if (!station.Consumes(resource))
                continue;

            int resourceIndex = (int)resource;
            int canUnload = Mathf.Min(cargo[resourceIndex].Amount, station.GetDemandAmount(resource));

            for (int i = 0; i < canUnload; i++)
            {
                yield return new WaitForSeconds(config.TimePerUnloadSec / TimeManager.Instance.TimeMultiplier);

                cargo[resourceIndex].Amount--;
                UpdateCargoVisualizers();
                FinanceManager.Instance.SellResource(resource);
                station.TrySatisfyDemand(resource, 1);
            }
        }

        int free = totalCapacity - CargoCount();
        if (free > 0)
        {
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                if (!station.Produces(resource))
                    continue;
                if (onlyLoadRequested && GlobalDemand.Outstanding[(int)resource].Amount <= 0)
                    continue;

                while (free > 0 && station.GetSupplyAmount(resource) > 0)
                {
                    yield return new WaitForSeconds(config.TimePerLoadSec / TimeManager.Instance.TimeMultiplier);

                    if (!station.TryTakeSupply(resource, 1))
                        break;

                    cargo[(int)resource].Amount++;
                    UpdateCargoVisualizers();
                    free--;
                }
            }
        }

        if (idx == worldPts.Count - 1 || idx == 0)
            dir = -dir;
        dwelling = false;
    }
    
    public void UpgradeCapacity(int delta) => totalCapacity = Mathf.Max(0, totalCapacity + delta);
    public void UpgradeSpeed(float mul) => speedUnitsPerSec *= Mathf.Max(0.1f, mul);
    public void SetSpeedLevel(int lvl)
    {
        SpeedLevel = lvl;
        switch (SpeedLevel)
        {
            case 1:
                speedUnitsPerSec = 0.6f;
                break;
            case 2:
                speedUnitsPerSec = 0.8f;
                break;
            case 3:
                speedUnitsPerSec = 1.2f;
                break;
        }
    }

    public void TryAddWagon()
    {
        if (wagons.Count >= maxWagons)
            return;

        var newWagon = Instantiate(wagonPrefab);
        newWagon.Init(this, wagons.Count);

        newWagon.transform.position = transform.position;
        wagons.Add(newWagon);
        CargoVisualizer wagonVisualizer = newWagon.GetComponent<CargoVisualizer>();
        wagonVisualizers.Add(wagonVisualizer);
        UpgradeCapacity(6);
    }

    public ResourceAmount[] Manifest() => cargo;

    public void SetSelectedVisual(bool isSelected)
    {
        if (bodyRenderer != null)
            bodyRenderer.color = isSelected ? selectedColor : normalColor;
    }

    public Vector3 GetWagonPosition(int wagonIndex, float distance)
    {
        float trainDistance = distance * (wagonIndex + 1);

        if (positionHistory.Count < 2)
            return transform.position;

        int idx = Mathf.Clamp(Mathf.RoundToInt(trainDistance / (speedUnitsPerSec * TimeManager.Instance.CustomDeltaTime)), 0, positionHistory.Count - 1);
        return positionHistory[idx];
    }

    public Vector3 GetWagonDirection(int wagonIndex, float distance)
    {
        float trainDistance = distance * (wagonIndex + 1);
        if (positionHistory.Count < 2)
            return transform.up;

        int idx = Mathf.Clamp(Mathf.RoundToInt(trainDistance / (speedUnitsPerSec * TimeManager.Instance.CustomDeltaTime)), 1, positionHistory.Count - 1);
        return (positionHistory[idx - 1] - positionHistory[idx]).normalized;
    }
    private void OnDestroy()
    {
        Debug.Log("Train destroyed");
    }

    private void EnsureCargoInitialized()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        if (cargo == null || cargo.Length != resourceTypes.Length)
            cargo = new ResourceAmount[resourceTypes.Length];

        for (int index = 0; index < resourceTypes.Length; index++)
        {
            if (cargo[index].Type != resourceTypes[index])
                cargo[index] = new ResourceAmount(resourceTypes[index], Mathf.Max(0, cargo[index].Amount));
        }
    }

    private void UpdateCargoVisualizers()
    {
        if (visualizer != null)
            visualizer.ShowCargo(BuildCargoSlice(0, trainHeadCapacity));

        for (int wagonIndex = 0; wagonIndex < wagonVisualizers.Count; wagonIndex++)
        {
            CargoVisualizer wagonVisualizer = wagonVisualizers[wagonIndex];
            if (wagonVisualizer == null)
                continue;

            int sliceStart = trainHeadCapacity + wagonIndex * 6;
            wagonVisualizer.ShowCargo(BuildCargoSlice(sliceStart, 6));
        }
    }

    private ResourceAmount[] BuildCargoSlice(int startIndex, int capacity)
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        ResourceAmount[] slice = new ResourceAmount[resourceTypes.Length];

        for (int index = 0; index < resourceTypes.Length; index++)
            slice[index] = new ResourceAmount(resourceTypes[index]);

        int remainingToSkip = Mathf.Max(0, startIndex);
        int remainingToTake = Mathf.Max(0, capacity);

        foreach (ResourceType resourceType in resourceTypes)
        {
            int cargoAmount = cargo[(int)resourceType].Amount;

            if (remainingToSkip >= cargoAmount)
            {
                remainingToSkip -= cargoAmount;
                continue;
            }

            int visibleAmount = Mathf.Min(cargoAmount - remainingToSkip, remainingToTake);
            slice[(int)resourceType].Amount = visibleAmount;
            remainingToTake -= visibleAmount;
            remainingToSkip = 0;

            if (remainingToTake <= 0)
                break;
        }

        return slice;
    }
}
