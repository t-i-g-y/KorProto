using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RailBuilderController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private Camera Cam;
    [SerializeField] private Grid ParentGrid;
    [SerializeField] private Tilemap Land;
    [SerializeField] private Tilemap Water;
    [SerializeField] private Tilemap Rail;
    [SerializeField] private Tilemap Highlight;
    [SerializeField] private RailPainter Painter;
    [SerializeField] private RailTopology Topology;
    
    [Header("Tiles")]
    [SerializeField] private TileBase RailTile;
    [SerializeField] private TileBase HighlightTile;
    
    [Header("Trains")]
    [SerializeField] private Train TrainPrefab;

    [Header("UI")]
    [SerializeField] private GameObject ConfirmPanel;
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button CancelButton;

    private List<Vector3Int> highlightPath = new();
    private bool isBuilding = false;
    private bool awaitingConfirm = false;

    private void Awake()
    {
        ConfirmPanel.SetActive(false);
        ConfirmButton.onClick.AddListener(ConfirmBuild);
        CancelButton.onClick.AddListener(CancelBuild);
    }

    private bool IsPointerOverUI() => EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    private bool IsLand(Vector3Int cell) => Land.HasTile(cell) && !Water.HasTile(cell);

    private Vector3Int MouseToCell()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return Vector3Int.zero;
        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 world = Cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Cam.transform.position.z * -1f));
        return ParentGrid.WorldToCell(world);
    }

    private void ClearHighlight()
    {
        foreach (var cell in highlightPath)
            Highlight.SetTile(cell, null);
        highlightPath.Clear();
    }

    private void Update()
    {
        if (awaitingConfirm)
            return;
        if (IsPointerOverUI())
            return;

        var mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.rightButton.wasPressedThisFrame)
            Debug.Log(MouseToCell());
        if (!isBuilding && mouse.leftButton.wasPressedThisFrame)
        {
            var start = MouseToCell();
            if (!IsLand(start))
                return;
            isBuilding = true;
            ClearHighlight();
            highlightPath.Add(start);
            Highlight.SetTile(start, HighlightTile);
            return;
        }

        if (isBuilding && mouse.leftButton.isPressed)
        {
            var cur = MouseToCell();
            if (cur == highlightPath[^1])
                return;

            if (IsLand(cur) && HexCoords.AreNeighbors(highlightPath[^1], cur))
            {
                if (highlightPath.Count >= 2 && cur == highlightPath[^2])
                {
                    Highlight.SetTile(highlightPath[^1], null);
                    highlightPath.RemoveAt(highlightPath.Count - 1);
                }
                else if (!highlightPath.Contains(cur))
                {
                    highlightPath.Add(cur);
                    Highlight.SetTile(cur, HighlightTile);
                }
            }
            return;
        }

        if (isBuilding && mouse.leftButton.wasReleasedThisFrame)
        {
            isBuilding = false;
            if (highlightPath.Count >= 2)
            {
                awaitingConfirm = true;
                ConfirmPanel.SetActive(true);
            }
            else
            {
                ClearHighlight();
            }
        }
    }

    private void ConfirmBuild()
    {
        RailLine line = RailManager.Instance.CreateLine(highlightPath);
        Painter.PaintRails(line);
        /*
        Topology.AddLine(highlightPath, cell =>
        {
            var dirs = Topology.GetDirs(cell);
            Painter.RepaintCell(cell, dirs);
        });
        */

        if (TrainPrefab)
        {
            var pts = new List<Vector3>(highlightPath.Count);
            foreach (var cell in highlightPath)
                pts.Add(Land.GetCellCenterWorld(cell));
            var train = Instantiate(TrainPrefab);
            train.SetPath(pts, pingPong: true);
        }

        ClearHighlight();
        ConfirmPanel.SetActive(false);
        awaitingConfirm = false;
    }

    private void CancelBuild()
    {
        ClearHighlight();
        ConfirmPanel.SetActive(false);
        awaitingConfirm = false;
    }
}