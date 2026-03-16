using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrainManager : MonoBehaviour
{
    public static TrainManager Instance { get; private set; }

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
}

