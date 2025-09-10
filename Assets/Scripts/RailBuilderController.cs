using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class RailBuilderController : MonoBehaviour
{
    [Header("Scene refs")]
    [SerializeField] private Camera Cam;
    [SerializeField] private Grid ParentGrid;
    [SerializeField] private Tilemap Land;
    [SerializeField] private Tilemap Water;
    [SerializeField] private Tilemap Rail;
    [SerializeField] private Tilemap Highlight;

    [Header("Tiles")]
    [SerializeField] private TileBase RailTile;
    [SerializeField] private TileBase HighlightTile;

    [Header("Systems")]
    [SerializeField] private RailPathFinder Pathfinder;
    [SerializeField] private Train TrainPrefab;

    Vector3Int? dragStartCell = null;
    List<Vector3Int> currentGhost = new();

    bool IsPointerOverUI() => EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    bool IsLand(Vector3Int cell) => Land.HasTile(cell) && !Water.HasTile(cell);

    Vector3Int MouseToCell()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return Vector3Int.zero;

        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 world = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Cam.transform.position.z * -1f));
        return ParentGrid.WorldToCell(world);
    }

    void ClearGhost()
    {
        foreach (var c in currentGhost)
            Highlight.SetTile(c, null);
        currentGhost.Clear();
    }

    void DrawGhost(List<Vector3Int> path)
    {
        ClearGhost();
        foreach (var c in path)
            Highlight.SetTile(c, HighlightTile);
        currentGhost.AddRange(path);
    }

    void PaintRail(List<Vector3Int> path)
    {
        foreach (var c in path)
            Rail.SetTile(c, RailTile);
    }

    void Update()
    {
        if (IsPointerOverUI())
            return;
        var mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Mouse pressed");
            var cell = MouseToCell();
            Debug.Log("Cell clicked: " + cell);
            dragStartCell = IsLand(cell) ? cell : (Vector3Int?)null;
            ClearGhost();
        }

        if (dragStartCell.HasValue && mouse.leftButton.isPressed)
        {
            var end = MouseToCell();
            if (!IsLand(end))
            {
                ClearGhost();
                return;
            }

            var path = Pathfinder.FindShortestPath(dragStartCell.Value, end);
            if (path.Count > 1)
                DrawGhost(path);
            else ClearGhost();
        }

        if (dragStartCell.HasValue && mouse.leftButton.wasReleasedThisFrame)
        {
            var end = MouseToCell();
            if (IsLand(end))
            {
                var path = Pathfinder.FindShortestPath(dragStartCell.Value, end);
                if (path.Count > 1)
                {
                    PaintRail(path);
                    SpawnTrainAlong(path);
                }
            }
            dragStartCell = null;
            ClearGhost();
        }
    }

    void SpawnTrainAlong(List<Vector3Int> path)
    {
        var pts = new List<Vector3>();
        foreach (var c in path)
            pts.Add(Land.GetCellCenterWorld(c));

        var t = Instantiate(TrainPrefab);
        t.SetPath(pts, pingPong:true);
    }
}

