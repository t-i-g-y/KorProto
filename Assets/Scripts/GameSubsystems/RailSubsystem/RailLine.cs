using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

public class RailLine
{
    public int ID { get; }
    public readonly List<Vector3Int> Cells;
    public readonly Vector3Int Start;
    public readonly Vector3Int End;
    public int Length => Cells.Count;
    public List<Train> AssignedTrains;
    public int MaxTrainCount;
    public bool CanAddTrain => AssignedTrains.Count < MaxTrainCount;

    public RailLine(int id, List<Vector3Int> cells)
    {
        ID = id;
        Cells = new List<Vector3Int>(cells);
        Start = cells[0];
        End = cells[^1];
        AssignedTrains = new List<Train>();
        MaxTrainCount = 2;
    }

    public override string ToString()
    {
        StringBuilder output = new StringBuilder($"Line {ID}:");
        for (int i = 0; i < Length; i++)
            output.Append($" {Cells[i]}");
        output.Append(";");
        return output.ToString();
    }

    private void OnDestroy()
    {
        foreach (var train in AssignedTrains)
        {
            train.gameObject.SetActive(false);
            train.ChangeSpeed(0f);
            UnityEngine.Object.Destroy(train.gameObject);
        }
        AssignedTrains.Clear();
    }

    public void AddTrain(Train train)
    {
        if (train == null || AssignedTrains.Contains(train))
            return;

        AssignedTrains.Add(train);
    }

    public void RemoveTrain(Train train)
    {
        if (train == null)
            return;

        AssignedTrains.Remove(train);
    }

    public float GetRoutingSpeed()
    {
        float best = 0f;

        for (int i = 0; i < AssignedTrains.Count; i++)
        {
            Train train = AssignedTrains[i];
            if (train == null || !train.IsOperational || train.IsBroken)
                continue;

            best = Mathf.Max(best, train.Speed);
        }

        return best;
    }

    #region save subsystem
    public RailLineSaveData GetSaveData()
    {
        return new RailLineSaveData { ID = ID, cells = new List<Vector3Int>(Cells)};
    }
    #endregion
}
