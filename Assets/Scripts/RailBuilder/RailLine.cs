using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class RailLine
{
    public int ID { get; }
    public readonly List<Vector3Int> Cells;
    public readonly Vector3Int Start;
    public readonly Vector3Int End;
    public int Length => Cells.Count;
    public Train AssignedTrain;

    public RailLine(int id, List<Vector3Int> cells)
    {
        ID = id;
        Cells = new List<Vector3Int>(cells);
        Start = cells[0];
        End = cells[^1];
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
        AssignedTrain.gameObject.SetActive(false);
        AssignedTrain.UpgradeSpeed(0);
    }
}
