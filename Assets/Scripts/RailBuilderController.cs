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
    [SerializeField] private RailPainter painter;
    [SerializeField] private RailTopology topology;
    
    [Header("Tiles")]
    [SerializeField] private TileBase railTile;
    [SerializeField] private TileBase ghostTile;
    
    [Header("Trains")]
    [SerializeField] private Train trainPrefab;

    [Header("UI")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private List<Vector3Int> ghostPath = new();
    private bool isBuilding = false;
    private bool awaitingConfirm = false;

    private void Awake()
    {
        confirmPanel.SetActive(false);
        confirmButton.onClick.AddListener(ConfirmBuild);
        cancelButton.onClick.AddListener(CancelBuild);
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
        foreach (var cell in ghostPath)
            Highlight.SetTile(cell, null);
        ghostPath.Clear();
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
            ghostPath.Add(start);
            Highlight.SetTile(start, ghostTile);
            return;
        }

        if (isBuilding && mouse.leftButton.isPressed)
        {
            var cur = MouseToCell();
            if (cur == ghostPath[^1])
                return;

            if (IsLand(cur) && HexCoords.AreNeighbors(ghostPath[^1], cur))
            {
                if (ghostPath.Count >= 2 && cur == ghostPath[^2])
                {
                    Highlight.SetTile(ghostPath[^1], null);
                    ghostPath.RemoveAt(ghostPath.Count - 1);
                }
                else if (!ghostPath.Contains(cur))
                {
                    ghostPath.Add(cur);
                    Highlight.SetTile(cur, ghostTile);
                }
            }
            return;
        }

        if (isBuilding && mouse.leftButton.wasReleasedThisFrame)
        {
            isBuilding = false;
            if (ghostPath.Count >= 2)
            {
                awaitingConfirm = true;
                confirmPanel.SetActive(true);
            }
            else
            {
                ClearHighlight();
            }
        }
    }

    private void ConfirmBuild()
    {
        RailLine line = RailManager.Instance.CreateLine(ghostPath);
        painter.PaintRails(line, false);

        if (trainPrefab)
        {
            var pts = new List<Vector3>(ghostPath.Count);
            foreach (var cell in ghostPath)
                pts.Add(Land.GetCellCenterWorld(cell));
            var train = Instantiate(trainPrefab);
            train.SetPath(pts);
        }

        ClearHighlight();
        confirmPanel.SetActive(false);
        awaitingConfirm = false;
    }

    private void CancelBuild()
    {
        ClearHighlight();
        confirmPanel.SetActive(false);
        awaitingConfirm = false;
    }
}