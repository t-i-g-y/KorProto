using System;
using System.Collections.Generic;
using UnityEngine;

// DEPRECATED
public class RailTopology : MonoBehaviour
{
    private readonly Dictionary<Vector3Int, HashSet<int>> dirsByCell = new();
    public IReadOnlyCollection<int> GetDirs(Vector3Int cell) => dirsByCell.TryGetValue(cell, out var set) ? set : (IReadOnlyCollection<int>)Array.Empty<int>();

    public void AddLine(List<Vector3Int> path, Action<Vector3Int> onCellChanged)
    {
        if (path == null || path.Count < 2)
            return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];
            int dAB = HexCoords.DirIndex(a, b);
            int dBA = HexCoords.DirIndex(b, a);
            if (dAB < 0 || dBA < 0)
                continue;

            if (!dirsByCell.TryGetValue(a, out var setA))
            {
                setA = new HashSet<int>();
                dirsByCell[a] = setA;
            }
            if (!dirsByCell.TryGetValue(b, out var setB))
            {
                setB = new HashSet<int>();
                dirsByCell[b] = setB;
            }

            if (setA.Add(dAB))
                onCellChanged?.Invoke(a);
            if (setB.Add(dBA))
                onCellChanged?.Invoke(b);
        }
    }

    public void RemoveLine(List<Vector3Int> path, Action<Vector3Int> onCellChanged)
    {
        if (path == null || path.Count < 2)
            return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];
            int dAB = HexCoords.DirIndex(a, b);
            int dBA = HexCoords.DirIndex(b, a);
            if (dAB < 0 || dBA < 0)
                continue;

            if (dirsByCell.TryGetValue(a, out var setA) && setA.Remove(dAB))
                onCellChanged?.Invoke(a);
            if (dirsByCell.TryGetValue(b, out var setB) && setB.Remove(dBA))
                onCellChanged?.Invoke(b);

            if (setA != null && setA.Count == 0)
                dirsByCell.Remove(a);
            if (setB != null && setB.Count == 0)
                dirsByCell.Remove(b);
        }
    }
}

