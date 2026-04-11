using System.Collections.Generic;
using UnityEngine;

public class RailAnchorRegistry : MonoBehaviour
{
    public static RailAnchorRegistry Instance { get; private set; }
    [SerializeField] private List<RailAnchor> anchors = new();
    private Dictionary<Vector3Int, RailAnchor> anchorsByCell = new();
    [SerializeField] private RailAnchor anchorPrefab;
    [SerializeField] private Grid parentGrid;
    public List<RailAnchor> Anchors => anchors;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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

    #region save subsystem
    public RailAnchorRegistrySaveData GetSaveData()
    {
        var data = new RailAnchorRegistrySaveData();

        foreach (var anchor in anchorsByCell.Values)
        {
            if (anchor == null)
                continue;

            data.anchors.Add(anchor.GetSaveData());
        }

        return data;
    }

    public void LoadFromSaveData(RailAnchorRegistrySaveData data)
    {
        ClearAll();

        if (data == null)
            return;

        if (anchorPrefab == null)
        {
            Debug.LogError("anchorPrefab not assigned in RailAnchorRegistry");
            return;
        }

        foreach (var anchorData in data.anchors)
        {
            Vector3Int cell = anchorData.cell;

            Vector3 worldPos = parentGrid.GetCellCenterWorld(cell);

            RailAnchor anchor = Instantiate(anchorPrefab, worldPos, Quaternion.identity);
            anchor.LoadFromSaveData(anchorData);

            anchorsByCell[cell] = anchor;
        }
    }
    public void ClearAll()
    {
        foreach (var anchor in anchorsByCell.Values)
        {
            if (anchor != null)
                Destroy(anchor.gameObject);
        }

        anchorsByCell.Clear();
    }
    #endregion
}
