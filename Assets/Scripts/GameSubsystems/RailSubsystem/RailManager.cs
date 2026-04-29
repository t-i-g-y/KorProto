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
        public Dictionary<Vector3Int, List<RailEdge>> Adjacency = new();
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
    private Dictionary<Vector3Int, int> nodeToIsland = new();
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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

        Vector3Int start = line.Start;
        Vector3Int end = line.End;

        if (SelectedLine == line)
        {
            InternalDeselect(line);
            LineDeselected?.Invoke(line);
        }

        for (int i = line.AssignedTrains.Count - 1; i >= 0; i--)
            TrainManager.Instance.RemoveTrain(line.AssignedTrains[i]);
            
        if (!Lines.Remove(line))
            Lines.RemoveAll(l => l.ID == line.ID);


        LineRemoved?.Invoke(line);

        RebuildConnectivity();
        TopologyChanged?.Invoke();
        ActiveNetworkChanged?.Invoke();
        CheckRemoveRelay(start);
        CheckRemoveRelay(end);
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

    public List<RailLine> GetLinesAtCell(Vector3Int cell)
    {
        List<RailLine> result = new();

        foreach (var line in Lines)
        {
            if (line == null)
                continue;

            if (line.Cells.Contains(cell))
                result.Add(line);
        }

        return result;
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
    public bool TryGetShortestPathFirstHop(Vector3Int from, Vector3Int to, out Vector3Int firstHop, out float totalCost)
    {
        firstHop = default;
        totalCost = 0f;

        if (from == to)
            return false;

        if (!TryGetIslandForNode(from, out NetworkIsland islandFrom))
            return false;

        if (!TryGetIslandForNode(to, out NetworkIsland islandTo))
            return false;

        if (islandFrom.ID != islandTo.ID)
            return false;

        if (!IsIslandActive(islandFrom))
            return false;

        if (!islandFrom.Adjacency.ContainsKey(from) || !islandFrom.Nodes.Contains(to))
            return false;

        Dictionary<Vector3Int, float> dist = new();
        Dictionary<Vector3Int, Vector3Int> prev = new();
        HashSet<Vector3Int> visited = new();

        List<(Vector3Int node, float cost)> queue = new()
        {
            (from, 0f)
        };
        dist[from] = 0f;
        while (queue.Count > 0)
        {
            queue.Sort((a, b) => a.cost.CompareTo(b.cost));

            var current = queue[0];
            queue.RemoveAt(0);

            Vector3Int currentNode = current.node;

            if (!visited.Add(currentNode))
                continue;

            if (currentNode == to)
                break;

            if (!islandFrom.Adjacency.TryGetValue(currentNode, out var edges))
                continue;

            foreach (RailEdge edge in edges)
            {
                if (visited.Contains(edge.To))
                    continue;

                float speed = edge.Line.GetRoutingSpeed();
                
                float nextCost = speed > 0f ? dist[currentNode] + edge.Cost / speed : float.MaxValue;

                if (!dist.TryGetValue(edge.To, out float oldCost) || nextCost < oldCost)
                {
                    dist[edge.To] = nextCost;
                    prev[edge.To] = currentNode;

                    queue.Add((edge.To, nextCost));
                }
            }
        }

        if (!dist.TryGetValue(to, out totalCost))
            return false;

        Vector3Int walk = to;
        while (prev.TryGetValue(walk, out Vector3Int previous))
        {
            if (previous == from)
            {
                firstHop = walk;
                return true;
            }

            walk = previous;
        }

        return false;
    }

    public bool IsFirstHopOnCurrentLine(RailLine line, Vector3Int currentCell, Vector3Int destinationCell, out float totalCost)
    {
        totalCost = 0;

        if (line == null)
            return false;

        if (!TryGetTwinnedEndpoint(line, currentCell, out Vector3Int twinnedCell))
            return false;

        if (!TryGetShortestPathFirstHop(currentCell, destinationCell, out Vector3Int firstHop, out totalCost))
            return false;

        return firstHop == twinnedCell;
    }

    public bool TryGetTwinnedEndpoint(RailLine line, Vector3Int current, out Vector3Int other)
    {
        other = default;

        if (line == null)
            return false;

        if (line.Start == current)
        {
            other = line.End;
            return true;
        }

        if (line.End == current)
        {
            other = line.Start;
            return true;
        }

        return false;
    }
    private void InternalDeselect(RailLine line)
    {
        if (painter == null || line == null)
            return;

        SelectedLine = null;
        painter.PaintRails(line, false);
    }

    private bool TryGetIslandForNode(Vector3Int node, out NetworkIsland island)
    {
        island = null;

        if (!nodeToIsland.TryGetValue(node, out int islandID))
            return false;

        return islands.TryGetValue(islandID, out island);
    }

    private bool IsIslandActive(NetworkIsland island)
    {
        return island != null && island.HasAnchor && !island.IsCollapsed;
    }

    private void RebuildConnectivity()
    {
        nextIslandID = 0;
        buildConnectedCells.Clear();
        activeLines.Clear();
        lineToIsland.Clear();
        islands.Clear();
        nodeToIsland.Clear();
        inspectorIslands.Clear();
        

        Dictionary<Vector3Int, List<RailLine>> nodeToLines = BuildNodeToLines();

        HashSet<RailLine> visitedLines = new();

        foreach (var line in Lines)
        {
            if (line == null || visitedLines.Contains(line))
                continue;

            NetworkIsland island = new NetworkIsland
            {
                ID = nextIslandID++
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
            BuildIslandAdjacency(island);
            island.HasAnchor = IslandHasAnchor(island);
            islands[island.ID] = island;
            inspectorIslands.Add(island);
        }

        foreach (var island in islands.Values)
        {
            bool active = island.HasAnchor && !island.IsCollapsed;

            

            if (active)
            {
                foreach (var node in island.Nodes)
                    buildConnectedCells.Add(node);
                foreach (var line in island.Lines)
                    activeLines.Add(line);
            }
        }
    }

    private void BuildIslandAdjacency(NetworkIsland island)
    {
        island.Adjacency.Clear();

        foreach (RailLine line in island.Lines)
        {
            if (line == null)
                continue;

            AddIslandEdge(island, line.Start, line.End, line, line.Length);
            AddIslandEdge(island, line.End, line.Start, line, line.Length);
        }
    }

    private void AddIslandEdge(NetworkIsland island, Vector3Int from, Vector3Int to, RailLine line, int cost)
    {
        if (!island.Adjacency.TryGetValue(from, out var edges))
        {
            edges = new List<RailEdge>();
            island.Adjacency[from] = edges;
        }

        edges.Add(new RailEdge(to, line, cost));
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
        nodeToIsland[node] = island.ID;
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

    private void CheckRemoveRelay(Vector3Int cell)
    {
        if (RelayStopRegistry.Instance == null)
            return;

        if (!RelayStopRegistry.Instance.IsRelayCell(cell))
            return;

        if (IsCellInActiveNetwork(cell))
            return;

        RelayStopRegistry.Instance.RemoveIfExists(cell);
    }

    private void ClearAll()
    {
        foreach (var line in Lines)
        {
            if (line != null && painter != null)
                painter.UnpaintRails(line);
        }

        Lines.Clear();
        SelectedLine = null;

        buildConnectedCells.Clear();
        activeLines.Clear();
        lineToIsland.Clear();
        islands.Clear();
        nodeToIsland.Clear();
        inspectorIslands.Clear();
    }

    #region save subsystem
    public RailManagerSaveData GetSaveData()
    {
        var data = new RailManagerSaveData { nextID = nextID };

        if (SelectedLine != null)
            data.selectedLineID = SelectedLine.ID;
        else
            data.selectedLineID = -1;

        foreach (var line in Lines)
        {
            if (line != null)
                data.lines.Add(line.GetSaveData());
        }

        return data;
    }

    public void LoadFromSaveData(RailManagerSaveData data)
    {
        ClearAll();

        if (data == null)
            return;

        nextID = data.nextID;

        foreach (var lineData in data.lines)
        {
            var line = new RailLine(lineData.ID, lineData.cells);
            Lines.Add(line);

            LineCreated?.Invoke(line);

            if (painter != null)
                painter.PaintRails(line, false);
        }

        RebuildConnectivity();
        TopologyChanged?.Invoke();
        ActiveNetworkChanged?.Invoke();

        if (data.selectedLineID != -1)
        {
            var selected = Lines.Find(l => l.ID == data.selectedLineID);
            if (selected != null)
            {
                SelectedLine = selected;

                if (painter != null)
                    painter.PaintRails(selected, true);

                LineSelected?.Invoke(selected);
            }
        }
    }
    #endregion
}
public struct RailEdge
{
    public Vector3Int To;
    public RailLine Line;
    public int Cost;

    public RailEdge(Vector3Int to, RailLine line, int cost)
    {
        To = to;
        Line = line;
        Cost = cost;
    }
}
