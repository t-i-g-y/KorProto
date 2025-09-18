using System;
using System.Collections.Generic;
using UnityEngine;

public class RailManager : MonoBehaviour
{
    private int nextID;
    public static RailManager Instance { get; private set; }
    public readonly Dictionary<int, RailLine> Lines = new();

    void Awake()
    {
        nextID = 0;
        Instance = this;
    }

    public RailLine CreateLine(List<Vector3Int> cells)
    {
        var line = new RailLine(nextID++, cells);
        Lines[line.ID] = line;
        return line;
    }

    public void RemoveLine(int id)
    {
        Lines.Remove(id);
    }

    public void PrintLines()
    {
        for (int i = 0; i < nextID; i++)
        {
            Debug.Log(Lines[i].ToString());
        }
    }
}
