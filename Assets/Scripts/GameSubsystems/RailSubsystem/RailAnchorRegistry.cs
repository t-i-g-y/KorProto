using System.Collections.Generic;
using UnityEngine;

public class RailAnchorRegistry : MonoBehaviour
{
    public static RailAnchorRegistry Instance { get; private set; }
    [SerializeField] private List<RailAnchor> anchors = new();
    public List<RailAnchor> Anchors => anchors;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RebuildList();
    }

    public void RebuildList()
    {
        anchors.Clear();
        anchors.AddRange(FindObjectsByType<RailAnchor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
    }

    public bool IsAnchorCell(Vector3Int cell)
    {
        foreach (var anchor in anchors)
        {
            if (anchor != null && anchor.Cell == cell)
                return true;
        }

        return false;
    }

    public bool HasAnyAnchor()
    {
        foreach (var anchor in anchors)
        {
            if (anchor != null)
                return true;
        }

        return false;
    }

    public List<Vector3Int> GetAllAnchorCells()
    {
        List<Vector3Int> cells = new();

        foreach (var anchor in anchors)
        {
            if (anchor != null)
                cells.Add(anchor.Cell);
        }

        return cells;
    }
}
