using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrainManager : MonoBehaviour
{
    public static TrainManager Instance { get; private set; }
    [SerializeField] private Train trainPrefab;
    [SerializeField] private TrainConfig trainConfig;
    [SerializeField] private Tilemap land;

    public List<Train> Trains= new();
    public Train SelectedTrain { get; private set; }

    private int nextID;

    public static event Action<Train, RailLine> TrainCreated;
    public static event Action<Train> TrainRemoved;
    public static event Action<Train> TrainSelected;
    public static event Action<Train> TrainDeselected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        nextID = 0;
    }

    public void RegisterTrain(Train train, RailLine line)
    {
        if (train == null || line == null)
            return;

        List<Vector3Int> tiles = new(line.Cells);
        List<Vector3> coords = new(tiles.Count);

        foreach (Vector3Int cell in tiles)
            coords.Add(land.GetCellCenterWorld(cell));

        train.Initialize(line, nextID, trainConfig);
        train.SetPath(tiles, coords);
        Trains.Add(train);
        line.AssignedTrain = train;
        
        train.RefreshOperationalState();
        train.TryHandleInitialEndpointLoad();

        TrainCreated?.Invoke(train, line);
        nextID++;
    }

    public void RemoveTrain(Train train)
    {
        if (train == null)
            return;

        if (SelectedTrain == train)
        {
            InternalDeselect(train);
            TrainDeselected?.Invoke(train);
        }

        if (train.AssignedLine != null)
            train.AssignedLine.AssignedTrain = null;

        Trains.Remove(train);
        TrainRemoved?.Invoke(train);
        Destroy(train.gameObject);
    }

    public void ToggleSelection(Train train)
    {
        if (train == null)
            return;

        if (SelectedTrain == train)
        {
            InternalDeselect(train);
            TrainDeselected?.Invoke(train);
            return;
        }

        if (SelectedTrain != null)
        {
            Train old = SelectedTrain;
            InternalDeselect(old);
            TrainDeselected?.Invoke(old);
        }

        SelectedTrain = train;
        train.SetSelectedVisual(true);
        TrainSelected?.Invoke(train);
    }

    private void InternalDeselect(Train train)
    {
        if (train == null)
            return;

        train.SetSelectedVisual(false);

        if (SelectedTrain == train)
            SelectedTrain = null;
    }

    #region 
    public TrainManagerSaveData GetSaveData()
    {
        var data = new TrainManagerSaveData { nextID = nextID };

        if (SelectedTrain != null)
            data.selectedTrainID = SelectedTrain.ID;

        foreach (var train in Trains)
        {
            if (train == null)
                continue;

            data.trains.Add(train.GetSaveData());
        }

        return data;
    }

    public void LoadFromSaveData(TrainManagerSaveData data)
    {
        ClearAll();

        if (data == null)
            return;

        nextID = data.nextID;

        foreach (var trainData in data.trains)
        {
            RailLine line = RailManager.Instance.Lines.Find(l => l.ID == trainData.assignedLineID);
            if (line == null)
                continue;

            Train train = Instantiate(trainPrefab);

            List<Vector3Int> tiles = new(line.Cells);
            List<Vector3> coords = new(tiles.Count);

            foreach (Vector3Int cell in tiles)
                coords.Add(land.GetCellCenterWorld(cell));

            train.Initialize(line, trainData.ID, trainConfig);
            train.SetPath(tiles, coords);
            train.LoadFromSaveData(trainData);

            Trains.Add(train);
            line.AssignedTrain = train;

            train.RefreshOperationalState();

            TrainCreated?.Invoke(train, line);
        }

        if (data.selectedTrainID.HasValue)
        {
            Train selected = Trains.Find(t => t != null && t.ID == data.selectedTrainID.Value);
            if (selected != null)
            {
                SelectedTrain = selected;
                selected.SetSelectedVisual(true);
                TrainSelected?.Invoke(selected);
            }
        }
    }
    public void ClearAll()
    {
        if (SelectedTrain != null)
        {
            InternalDeselect(SelectedTrain);
            TrainDeselected?.Invoke(SelectedTrain);
        }

        for (int i = Trains.Count - 1; i >= 0; i--)
        {
            Train train = Trains[i];
            if (train == null)
                continue;

            if (train.AssignedLine != null)
                train.AssignedLine.AssignedTrain = null;

            TrainRemoved?.Invoke(train);
            Destroy(train.gameObject);
        }

        Trains.Clear();
        SelectedTrain = null;
    }
    #endregion
}

