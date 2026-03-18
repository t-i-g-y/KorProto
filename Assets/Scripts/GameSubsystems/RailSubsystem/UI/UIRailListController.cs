using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RailListUIController : MonoBehaviour
{
    [SerializeField] private Button viewListButton;
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject listEntryPrefab;
    [SerializeField] private Transform listContent;

    private readonly Dictionary<int, RailLineUIEntry> entries = new();

    void Awake()
    {
        if (listPanel != null)
            listPanel.SetActive(false);

        if (viewListButton != null)
            viewListButton.onClick.AddListener(() => TogglePanel(listPanel));
    }

    private void OnEnable()
    {
        RailManager.LineCreated += OnLineCreated;
        RailManager.LineRemoved += OnLineRemoved;
        RailManager.LineSelected += OnLineSelected;
        RailManager.LineDeselected += OnLineDeselected;
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= OnLineCreated;
        RailManager.LineRemoved -= OnLineRemoved;
        RailManager.LineSelected -= OnLineSelected;
        RailManager.LineDeselected -= OnLineDeselected;
    }

    private void OnLineCreated(RailLine line)
    {
        if (listEntryPrefab == null || listContent == null || line == null)
            return;

        GameObject entryObject = Instantiate(listEntryPrefab, listContent);
        var entry = entryObject.GetComponent<RailLineUIEntry>();

        if (entry == null)
        {
            Debug.LogError("No LineEntry component");
            return;
        }

        entry.Init(line);
        entry.OnSelectClicked += HandleSelectClicked;
        entry.OnDeleteClicked += HandleDeleteClicked;

        entries[line.ID] = entry;
    }

    private void OnLineRemoved(RailLine line)
    {
        if (line == null)
            return;
        
        if (entries.TryGetValue(line.ID, out var entry))
        {
            entry.OnSelectClicked -= HandleSelectClicked;
            entry.OnDeleteClicked -= HandleDeleteClicked;
            Destroy(entry.gameObject);
            entries.Remove(line.ID);
        }
    }

    private void OnLineSelected(RailLine line)
    {
        if (line == null)
            return;
         Debug.Log($"UIRailListController.OnLineSelected: {line.ID}");
        if (entries.TryGetValue(line.ID, out var entry))
            entry.SetSelected(true);
    }

    private void OnLineDeselected(RailLine line)
    {
        if (line == null)
            return;
         Debug.Log($"UIRailListController.OnLineDeelected: {line.ID}");
        if (entries.TryGetValue(line.ID, out var entry))
            entry.SetSelected(false);
    }

    private void HandleSelectClicked(RailLineUIEntry entry)
    {
        if (entry == null || entry.ReferenceLine == null) 
            return;
        Debug.Log($"UIRailListController.HandleSelectClicked: entry {entry.ReferenceLine.ID}, IsSelected={entry.IsSelected}");
        RailManager.Instance.ToggleSelection(entry.ReferenceLine);
    }

    private void HandleDeleteClicked(RailLineUIEntry entry)
    {
        if (entry == null || entry.ReferenceLine == null) 
            return;

        RailManager.Instance.RemoveLine(entry.ReferenceLine);
    }

    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
}
