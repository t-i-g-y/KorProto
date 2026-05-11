using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class TrainAddDragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image icon;
    [SerializeField] private RectTransform dragVisualParent;

    [Header("Scene")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap land;

    private Image dragVisual;
    private RailLine currentHoveredLine;
    private RailLine selectedByDragLine;

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (icon == null || canvas == null)
            return;

        dragVisual = Instantiate(icon, dragVisualParent != null ? dragVisualParent : canvas.transform);
        dragVisual.raycastTarget = false;
        dragVisual.transform.SetAsLastSibling();
        dragVisual.rectTransform.position = eventData.position;

        currentHoveredLine = null;
        selectedByDragLine = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragVisual != null)
            dragVisual.rectTransform.position = eventData.position;

        RailLine hoveredLine = GetLineUnderPointer(eventData);

        if (hoveredLine == null && selectedByDragLine != null && RailManager.Instance.SelectedLine == selectedByDragLine)
        {
            RailManager.Instance.ToggleSelection(selectedByDragLine);
            return;
        }

        if (hoveredLine == currentHoveredLine)
            return;

        currentHoveredLine = hoveredLine;

        if (currentHoveredLine != null)
        {
            if (RailManager.Instance.SelectedLine != currentHoveredLine)
                RailManager.Instance.ToggleSelection(currentHoveredLine);

            selectedByDragLine = currentHoveredLine;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        RailLine targetLine = GetLineUnderPointer(eventData);

        if (dragVisual != null)
            Destroy(dragVisual.gameObject);

        dragVisual = null;

        if (targetLine != null)
            TrainManager.Instance.TryCreateTrain(targetLine);

        if (targetLine != null && RailManager.Instance.SelectedLine == targetLine)
            RailManager.Instance.ToggleSelection(targetLine);

        if (selectedByDragLine != null && RailManager.Instance.SelectedLine == selectedByDragLine)
            RailManager.Instance.ToggleSelection(selectedByDragLine);
        
        currentHoveredLine = null;
        selectedByDragLine = null;
    }

    private RailLine GetLineUnderPointer(PointerEventData eventData)
    {
        if (grid == null || worldCamera == null || RailManager.Instance == null)
            return null;

        Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, -worldCamera.transform.position.z));

        Vector3Int cell = grid.WorldToCell(world);
        List<RailLine> lines = RailManager.Instance.GetLinesAtCell(cell);

        if (lines == null || lines.Count == 0)
            return null;

        RailLine selected = RailManager.Instance.SelectedLine;
        if (selected != null && lines.Contains(selected))
            return selected;

        return lines[0];
    }
}
