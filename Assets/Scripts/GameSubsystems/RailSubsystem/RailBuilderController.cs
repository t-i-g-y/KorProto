using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class RailBuilderController : MonoBehaviour
{
    [Header("Scene Objects")]
    [SerializeField] private Camera cam;
    [SerializeField] private Grid parentGrid;
    [SerializeField] private Tilemap land;
    [SerializeField] private Tilemap water;
    [SerializeField] private Tilemap ghost;
    [SerializeField] private RailPainter painter;
    [SerializeField] private HexRailNetwork system;
    [SerializeField] private GameConfig config;

    [Header("Tiles")]
    [SerializeField] private TileBase railTile;
    [SerializeField] private TileBase ghostTile;

    [Header("Build Limits")]
    [SerializeField] private int maxLineLength = 8;

    [Header("UI")]
    [SerializeField] private GameObject confirmHolder;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private float verticalOffset;
    [SerializeField] private GameObject lengthPanel;
    [SerializeField] private TMP_Text lengthText;
    [SerializeField] private Vector3 lengthPanelOffset = new Vector3(0f, 40f, 0f);

    private List<Vector3Int> ghostPath = new();
    private bool isBuilding = false;
    private bool awaitingConfirm = false;
    private List<RailLine> cycledLines = new();
    private int cycledLineIndex = 0;

    private void Awake()
    {
        confirmHolder.SetActive(false);
        if (lengthPanel != null)
            lengthPanel.SetActive(false);
        confirmButton.onClick.AddListener(ConfirmBuild);
        cancelButton.onClick.AddListener(CancelBuild);
    }

    private bool IsPointerOverUI() => EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    private bool IsLand(Vector3Int cell) => land.HasTile(cell) && !water.HasTile(cell);
    private bool IsWater(Vector3Int cell) => !land.HasTile(cell) && water.HasTile(cell);

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
        if (awaitingConfirm || IsPointerOverUI())
            return;

        var mouse = Mouse.current;
        if (mouse == null)
            return;

        HandleLineCycling();
        
        if (mouse.rightButton.wasPressedThisFrame)
            Debug.Log(MouseToCell());

        if (!isBuilding && mouse.leftButton.wasPressedThisFrame)
        {
            var start = MouseToCell();
            if (!IsLand(start))
                return;

            if (!RailManager.Instance.CanStartBuildFrom(start))
            {
                Debug.Log("Build must start from an anchor or connected railway");
                return;
            }

            TerrainType terrain = HexRailNetwork.Instance.GetTerrainType(start);

            if (!ResearchModifierSystem.Instance.CanBuildOn(terrain))
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

            if (ghostPath.Count >= 2 && cur == ghostPath[^2])
            {
                ghost.SetTile(ghostPath[^1], null);
                ghostPath.RemoveAt(ghostPath.Count - 1);
                painter.PaintGhostPath(ghostPath);
                UpdateLengthUI();
                return;
            }

            if (ghostPath.Count >= maxLineLength)
                return;

            TerrainType terrain = HexRailNetwork.Instance.GetTerrainType(cur);
            if (terrain == TerrainType.Ocean)
                return;

            if (!HexCoords.AreNeighbours(ghostPath[^1], cur))
                return;

            //bool canStep = (IsLand(cur) && (terrain != TerrainType.Mountain || canBuildMountainTunnel)) || (IsWater(cur) && ((terrain == TerrainType.Lake && canBuildLakeCrossing) || (terrain == TerrainType.Sea && canBuildSeaTunnel)));
            bool canStep = ResearchModifierSystem.Instance.CanBuildOn(terrain);
            if (!canStep)
                return;

            if (!ghostPath.Contains(cur))
            {
                ghostPath.Add(cur);
                ghost.SetTile(cur, ghostTile);
                painter.PaintGhostPath(ghostPath);
                UpdateLengthUI();
            }

            return;
        }

        if (isBuilding && mouse.leftButton.wasReleasedThisFrame)
        {
            isBuilding = false;

            if (ghostPath.Count < 2)
            {
                ClearHighlight();
                return;
            }

            if (ghostPath.Count > maxLineLength)
            {
                Debug.Log("Line too long");
                ClearHighlight();
                return;
            }

            if (HexRailNetwork.Instance.GetTerrainType(ghostPath[^1]) == TerrainType.Lake || HexRailNetwork.Instance.GetTerrainType(ghostPath[^1]) == TerrainType.Sea)
            {
                Debug.Log("Water tile cannot be endpoint");
                ClearHighlight();
                return;
            }

            if (!RailManager.Instance.CanAttachPath(ghostPath))
            {
                Debug.Log("New line must connect to the existing network");
                ClearHighlight();
                return;
            }

            if (HexRailNetwork.Instance.IsLineDuplicate(ghostPath))
            {
                Debug.Log("Can't create duplicate line");
                ClearHighlight();
                return;
            }

            awaitingConfirm = true;
            confirmHolder.SetActive(true);
            UpdateUIPanelPosition();
        }

        if (isBuilding || awaitingConfirm)
            UpdateLengthUI();
    }

    private void LateUpdate()
    {
        UpdateUIPanelPosition();
    }

    private void UpdateUIPanelPosition()
    {
        if (ghostPath == null || ghostPath.Count == 0 || cam == null || land == null)
            return;

        if (!isBuilding && !awaitingConfirm)
            return;

        Vector3 worldPos = land.GetCellCenterWorld(ghostPath[^1]);
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (confirmHolder != null && awaitingConfirm && confirmHolder.activeSelf)
            confirmHolder.transform.position = screenPos + new Vector3(0f, verticalOffset, 0f);

        if (lengthPanel != null && lengthPanel.activeSelf)
            lengthPanel.transform.position = screenPos + lengthPanelOffset;
    }
    private void ConfirmBuild()
    {
        RailLine line = RailManager.Instance.CreateLine(ghostPath);
        painter.PaintRails(line, false);

        CreateRelayIfNeeded(line.End);
        TrainManager.Instance.TryCreateTrain(line);

        ClearHighlight();
        confirmHolder.SetActive(false);
        lengthPanel.SetActive(false);
        awaitingConfirm = false;
    }

    private void CreateRelayIfNeeded(Vector3Int endpoint)
    {
        if (StationRegistry.TryGet(endpoint, out _))
            return;

        if (RailAnchorRegistry.Instance != null && RailAnchorRegistry.Instance.IsAnchorCell(endpoint))
            return;

        RelayStopRegistry.Instance?.GetOrCreate(endpoint, parentGrid.GetCellCenterWorld(endpoint));
    }

    private void CancelBuild()
    {
        ClearHighlight();
        confirmHolder.SetActive(false);
        awaitingConfirm = false;
        lengthPanel.SetActive(false);
    }

    private void UpdateLengthUI()
    {
        if (lengthPanel == null || lengthText == null)
            return;

        bool show = isBuilding || awaitingConfirm;
        lengthPanel.SetActive(show);

        if (!show)
            return;

        int used = ghostPath.Count;
        int remaining = Mathf.Max(0, maxLineLength - used);

        if (remaining > 4 && remaining <= maxLineLength)
            lengthText.color = Color.green;
        else if (remaining > 0)
            lengthText.color = Color.yellow;
        else
            lengthText.color = Color.red;

        lengthText.text = $"{remaining}";
    }

    public void ChangeMaxLineLength(int delta) => maxLineLength += delta;

    private void HandleLineCycling()
    {
        Vector3Int cell = MouseToCell();
        cycledLines = RailManager.Instance.GetLinesAtCell(cell);

        if (cycledLines.Count == 0)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            cycledLineIndex--;
            if (cycledLineIndex < 0)
                cycledLineIndex = cycledLines.Count - 1;

            RailManager.Instance.ToggleSelection(cycledLines[cycledLineIndex]);
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            cycledLineIndex++;
            if (cycledLineIndex >= cycledLines.Count)
                cycledLineIndex = 0;

            RailManager.Instance.ToggleSelection(cycledLines[cycledLineIndex]);
        }
    }
}