using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrainManager : MonoBehaviour
{
    public int NextID { get; private set; }
    public static TrainManager Instance { get; private set; }
    public readonly List<Train> Trains = new();
    public Train SelectedTrain { get; private set; }
    [SerializeField] private TrainConfig trainConfig;
    [SerializeField] private Tilemap land;
    public static event Action<Train, RailLine> TrainCreated;
    public static event Action<Train> TrainRemoved;
    public static event Action<Train> TrainSelected;
    public static event Action<Train> TrainDeselected;

    private void Awake()
    {
        NextID = 0;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterTrain(Train train, RailLine line)
    {
        if (train == null) 
            return;
    
        var ptsWorld = new List<Vector3>(line.Length);
            foreach (var c in line.Cells)
                ptsWorld.Add(land.GetCellCenterWorld(c));

        train.SetPath(ptsWorld, new List<Vector3Int>(line.Cells));
        train.config = trainConfig;
        train.onlyLoadRequested = true;
        train.AssignedLine = line;
        line.AssignedTrain = train;
        Trains.Add(train);
        TrainCreated?.Invoke(train, line);
        NextID++;
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

        for (int i = 0; i < train.Wagons.Count; i++)
        {
            Destroy(train.Wagons[i].gameObject);
            train.Wagons[i] = null;
        }
        train.Wagons.Clear();
        Trains.Remove(train);
        TrainRemoved?.Invoke(train);

        if (train.AssignedLine != null)
            train.AssignedLine.AssignedTrain = null;

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
            var old = SelectedTrain;
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

