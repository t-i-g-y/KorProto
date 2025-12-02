using System;
using System.Collections.Generic;
using UnityEngine;

public class RailManager : MonoBehaviour
{
    private int nextID;
    [SerializeField] private RailPainter painter;
    public static RailManager Instance { get; private set; }
    public readonly List<RailLine> Lines = new();
    public RailLine SelectedLine { get; private set; }
    public static event Action<RailLine> LineCreated;
    public static event Action<RailLine> LineRemoved;
    public static event Action<RailLine> LineSelected;
    public static event Action<RailLine> LineDeselected;

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
        LineCreated?.Invoke(line);
        return line;
    }

    public void RemoveLine(RailLine line)
    {
        if (SelectedLine == line)
        {
            InternalDeselect(line);
            LineDeselected?.Invoke(line);
        }

        RailSystem.Instance.RemoveRailData(line);
        painter.UnpaintRails(line);
        line.assignedTrain.gameObject.SetActive(false);
        line.assignedTrain.UpgradeSpeed(0);
        if (!Lines.Remove(line))
        {
            Lines.RemoveAll(l => l.ID == line.ID);
        }

        LineRemoved?.Invoke(line);
    }

    public void PrintLines()
    {
        for (int i = 0; i < nextID; i++)
        {
            Debug.Log(Lines[i].ToString());
        }
    }

    public void ToggleSelection(RailLine line)
    {
        if (line == null)
            return;
        
        if (SelectedLine == line)
        {
            InternalDeselect(line);
            painter.PaintRails(line, false);
            LineDeselected?.Invoke(line);
            return;
        }

        if (SelectedLine != null)
        {
            RailLine oldLine = SelectedLine;
            InternalDeselect(oldLine);
            LineDeselected?.Invoke(oldLine);
        }

        SelectedLine = line;
        painter.PaintRails(line, true);
        LineSelected?.Invoke(line);
    }

    public void ForceDeselect(RailLine line)
    {
        if (line == null)
            return;
        
        if (SelectedLine != line)
            return;
        
        InternalDeselect(line);
        LineDeselected?.Invoke(line);
    }

    private void InternalDeselect(RailLine line)
    {
        if (painter == null || line == null) 
            return;
        SelectedLine = null;
        painter.PaintRails(line, false);
    }

}
