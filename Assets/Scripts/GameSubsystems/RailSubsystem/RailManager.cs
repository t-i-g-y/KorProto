using System;
using System.Collections.Generic;
using UnityEngine;

public class RailManager : MonoBehaviour
{
    [Serializable]
    private class NetworkIsland
    {
        public int ID;
        public HashSet<Vector3Int> Nodes = new();
        public HashSet<RailLine> Lines = new();
        public bool HasAnchor;
        public float DisconnectTime;
        public bool IsCollapsed;
    }

    private int nextID;
    private int nextIslandID;

    [SerializeField] private RailPainter painter;
    [SerializeField] private float collapseDelaySeconds = 10f;

    public static RailManager Instance { get; private set; }

    public List<RailLine> Lines = new();
    public RailLine SelectedLine { get; private set; }

    private HashSet<Vector3Int> buildConnectedCells = new();
    private HashSet<RailLine> activeLines = new();
    private Dictionary<RailLine, int> lineToIsland = new();
    private Dictionary<int, NetworkIsland> islands = new();
    [SerializeField] private List<NetworkIsland> inspectorIslands = new();

    public HashSet<RailLine> ActiveLines => activeLines;

    public static event Action<RailLine> LineCreated;
    public static event Action<RailLine> LineRemoved;
    public static event Action<RailLine> LineSelected;
    public static event Action<RailLine> LineDeselected;
    public static event Action TopologyChanged;
    public static event Action ActiveNetworkChanged;

    private void Awake()
    {
        nextID = 0;
        nextIslandID = 0;

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

    private void Update()
    {
        UpdateIslandCollapseTimers();
    }

    public RailLine CreateLine(List<Vector3Int> cells)
    {
        var line = new RailLine(nextID++, cells);
        Lines.Add(line);

        LineCreated?.Invoke(line);

        RebuildConnectivity();
        TopologyChanged?.Invoke();
        ActiveNetworkChanged?.Invoke();

        return line;
    }

    public void RemoveLine(RailLine line)
    {
        if (line == null)
            return;

        if (SelectedLine == line)
        {
            InternalDeselect(line);
            LineDeselected?.Invoke(line);
        }

        TrainManager.Instance.RemoveTrain(line.AssignedTrain);

        if (!Lines.Remove(line))
            Lines.RemoveAll(l => l.ID == line.ID);

        LineRemoved?.Invoke(line);

        RebuildConnectivity();
        TopologyChanged?.Invoke();
        ActiveNetworkChanged?.Invoke();
    }

    public bool CanStartBuildFrom(Vector3Int cell)
    {
        if (Lines.Count == 0)
            return RailAnchorRegistry.Instance != null && RailAnchorRegistry.Instance.IsAnchorCell(cell);

        return buildConnectedCells.Contains(cell);
    }

    public bool CanAttachPath(List<Vector3Int> path)
    {
        if (path == null || path.Count < 2)
            return false;

        if (Lines.Count == 0)
            return RailAnchorRegistry.Instance != null && RailAnchorRegistry.Instance.IsAnchorCell(path[0]);

        return buildConnectedCells.Contains(path[0]) || buildConnectedCells.Contains(path[^1]);
    }

    public bool IsEndpointCell(Vector3Int cell)
    {
        if (RailAnchorRegistry.Instance != null && RailAnchorRegistry.Instance.IsAnchorCell(cell))
            return true;

        if (StationRegistry.TryGet(cell, out _))
            return true;

        if (RelayStopRegistry.Instance != null && RelayStopRegistry.Instance.IsRelayCell(cell))
            return true;

        return false;
    }

    public bool IsLineActive(RailLine line)
    {
        return line != null && activeLines.Contains(line);
    }

    public bool IsCellInActiveNetwork(Vector3Int cell)
    {
        foreach (var island in islands.Values)
        {
            if (!island.HasAnchor || island.IsCollapsed)
                continue;

            if (island.Nodes.Contains(cell))
                return true;
        }

        return false;
    }

    public void PrintLines()
    {
        foreach (var line in Lines)
            Debug.Log(line.ToString());
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
        if (line == null || SelectedLine != line)
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

    private void RebuildConnectivity()
    {
        buildConnectedCells.Clear();
        activeLines.Clear();
        lineToIsland.Clear();
        islands.Clear();
        inspectorIslands.Clear();

        Dictionary<Vector3Int, List<RailLine>> nodeToLines = BuildNodeToLines();

        HashSet<RailLine> visitedLines = new();

        foreach (var line in Lines)
        {
            if (line == null || visitedLines.Contains(line))
                continue;

            NetworkIsland island = new NetworkIsland
            {
                ID = (islands.Values.Count > 1 ? nextIslandID++ : 0)
            };

            Queue<RailLine> queue = new();
            queue.Enqueue(line);
            visitedLines.Add(line);

            while (queue.Count > 0)
            {
                RailLine current = queue.Dequeue();
                island.Lines.Add(current);
                lineToIsland[current] = island.ID;

                AddNode(current.Start, island);
                AddNode(current.End, island);

                foreach (var next in GetNeighborLines(current.Start, current, nodeToLines))
                {
                    if (visitedLines.Add(next))
                        queue.Enqueue(next);
                }

                foreach (var next in GetNeighborLines(current.End, current, nodeToLines))
                {
                    if (visitedLines.Add(next))
                        queue.Enqueue(next);
                }
            }

            island.HasAnchor = IslandHasAnchor(island);
            islands[island.ID] = island;
            inspectorIslands.Add(island);
        }

        foreach (var island in islands.Values)
        {
            bool active = island.HasAnchor && !island.IsCollapsed;

            foreach (var node in island.Nodes)
                buildConnectedCells.Add(node);

            if (active)
            {
                foreach (var line in island.Lines)
                    activeLines.Add(line);
            }
        }
    }

    private Dictionary<Vector3Int, List<RailLine>> BuildNodeToLines()
    {
        Dictionary<Vector3Int, List<RailLine>> result = new();

        foreach (var line in Lines)
        {
            if (line == null)
                continue;

            AddNodeLine(result, line.Start, line);
            AddNodeLine(result, line.End, line);
        }

        return result;
    }

    private void AddNodeLine(Dictionary<Vector3Int, List<RailLine>> dict, Vector3Int node, RailLine line)
    {
        if (!dict.TryGetValue(node, out var list))
        {
            list = new List<RailLine>();
            dict[node] = list;
        }

        list.Add(line);
    }

    private IEnumerable<RailLine> GetNeighborLines(Vector3Int node, RailLine self, Dictionary<Vector3Int, List<RailLine>> nodeToLines)
    {
        if (!nodeToLines.TryGetValue(node, out var lines))
            yield break;

        foreach (var line in lines)
        {
            if (line != null && line != self)
                yield return line;
        }
    }

    private void AddNode(Vector3Int node, NetworkIsland island)
    {
        island.Nodes.Add(node);
    }

    private bool IslandHasAnchor(NetworkIsland island)
    {
        if (RailAnchorRegistry.Instance == null)
            return false;

        foreach (var node in island.Nodes)
        {
            if (RailAnchorRegistry.Instance.IsAnchorCell(node))
                return true;
        }

        return false;
    }

    private void UpdateIslandCollapseTimers()
    {
        bool changed = false;

        foreach (var island in islands.Values)
        {
            if (island.HasAnchor)
            {
                if (island.IsCollapsed)
                {
                    island.IsCollapsed = false;
                    changed = true;
                }

                island.DisconnectTime = 0f;
                continue;
            }

            island.DisconnectTime += TimeManager.Instance.CustomDeltaTime;

            if (!island.IsCollapsed && island.DisconnectTime >= collapseDelaySeconds)
            {
                island.IsCollapsed = true;
                changed = true;
            }
        }

        if (changed)
        {
            RefreshActiveLines();
            ActiveNetworkChanged?.Invoke();
        }
    }

    private void RefreshActiveLines()
    {
        activeLines.Clear();

        foreach (var island in islands.Values)
        {
            if (!island.HasAnchor || island.IsCollapsed)
                continue;

            foreach (var line in island.Lines)
                activeLines.Add(line);
        }
    }
}