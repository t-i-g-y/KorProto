using System;
using System.Collections.Generic;
using UnityEngine;

public class RailManager : MonoBehaviour
{
    private int nextID;
    public static RailManager Instance { get; private set; }
    public readonly List<RailLine> Lines = new();
    [SerializeField] private RailPainter painter;

    void Awake()
    {
        nextID = 0;
        Instance = this;
    }

    public RailLine CreateLine(List<Vector3Int> cells)
    {
        var line = new RailLine(nextID++, cells);
        Lines.Add(line);
        RailSystem.Instance.AddRailData(line);
        return line;
    }

    public void RemoveLine(RailLine line)
    {
        RailSystem.Instance.RemoveRailData(line);
        painter.UnpaintRails(line);
        if (!Lines.Remove(line))
        {
            Lines.RemoveAll(l => l.ID == line.ID);
        }
    }

    public void PrintLines()
    {
        for (int i = 0; i < nextID; i++)
        {
            Debug.Log(Lines[i].ToString());
        }
    }
}
