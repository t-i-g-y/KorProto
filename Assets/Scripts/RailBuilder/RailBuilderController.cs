using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RailBuilderController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private Camera cam;
    [SerializeField] private Grid parentGrid;
    [SerializeField] private Tilemap land;
    [SerializeField] private Tilemap water;
    [SerializeField] private Tilemap ghost;
    [SerializeField] private RailPainter painter;
    [SerializeField] private RailSystem system;
    [SerializeField] private GameConfig config;
    
    [Header("Tiles")]
    [SerializeField] private TileBase railTile;
    [SerializeField] private TileBase ghostTile;
    
    [Header("Trains")]
    [SerializeField] private Train trainPrefab;

    [Header("UI")]
    [SerializeField] private GameObject confirmHolder;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private float verticalOffset;

    private List<Vector3Int> ghostPath = new();
    private bool isBuilding = false;
    private bool awaitingConfirm = false;

    private void Awake()
    {
        confirmHolder.SetActive(false);
        confirmButton.onClick.AddListener(ConfirmBuild);
        cancelButton.onClick.AddListener(CancelBuild);
    }

    private bool IsPointerOverUI() => EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    private bool IsLand(Vector3Int cell) => land.HasTile(cell) && !water.HasTile(cell);

    private Vector3Int MouseToCell()
    {
        var mouse = Mouse.current;
        if (mouse == null)
            return Vector3Int.zero;
        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.transform.position.z * -1f));
        return parentGrid.WorldToCell(world);
    }

    private void ClearHighlight()
    {
        foreach (var cell in ghostPath)
            ghost.SetTile(cell, null);
        ghostPath.Clear();
        painter.ClearGhost();
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
            painter.PaintGhostPath(ghostPath);
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
                    ghost.SetTile(ghostPath[^1], null);
                    ghostPath.RemoveAt(ghostPath.Count - 1);
                }
                else if (!ghostPath.Contains(cur))
                {
                    ghostPath.Add(cur);
                    ghost.SetTile(cur, ghostTile);
                }
                painter.PaintGhostPath(ghostPath);
            }
            return;
        }

        if (isBuilding && mouse.leftButton.wasReleasedThisFrame)
        {
            isBuilding = false;
            if (ghostPath.Count >= 2)
            {
                if (RailSystem.Instance.IsLineDuplicate(ghostPath))
                {
                    ClearHighlight();
                    Debug.Log("Can't create duplicate!");
                }
                else
                {
                    awaitingConfirm = true;
                    Vector3 offset = new Vector3(0, verticalOffset, 0);
                    confirmHolder.transform.position = cam.WorldToScreenPoint(land.GetCellCenterWorld(ghostPath[^1])) + offset;
                    confirmHolder.SetActive(true);
                }
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
            var ptsWorld = new List<Vector3>(ghostPath.Count);
            foreach (var c in ghostPath)
                ptsWorld.Add(land.GetCellCenterWorld(c));

            var train = Instantiate(trainPrefab);
            train.config = config;
            train.onlyLoadRequested = true;
            train.SetPath(ptsWorld, new List<Vector3Int>(ghostPath));
            line.assignedTrain = train;
        }

        ClearHighlight();
        confirmHolder.SetActive(false);
        awaitingConfirm = false;
    }

    private void CancelBuild()
    {
        ClearHighlight();
        confirmHolder.SetActive(false);
        awaitingConfirm = false;
    }
}