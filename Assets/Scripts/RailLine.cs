using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RailLine
{
    public int ID { get; }
    public readonly List<Vector3Int> Cells;
    public int Length => Cells.Count;

    public RailLine(int id, List<Vector3Int> cells)
    {
        ID = id;
        Cells = new List<Vector3Int>(cells);
    }

    public override string ToString()
    {
        StringBuilder output = new StringBuilder($"Line {ID}:");
        for (int i = 0; i < Length; i++)
            output.Append($" {Cells[i]}");
        output.Append(";");
        return output.ToString();
    }
}
