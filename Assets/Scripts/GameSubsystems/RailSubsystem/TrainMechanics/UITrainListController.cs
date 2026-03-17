using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITrainListController : MonoBehaviour
{
    [SerializeField] private Button viewListButton;
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject listEntryPrefab;
    [SerializeField] private Transform listContent;

    private readonly Dictionary<Train, UITrainLineEntry> entries = new();

    private void Awake()
    {
        if (listPanel != null)
            listPanel.SetActive(false);

        if (viewListButton != null)
            viewListButton.onClick.AddListener(() => TogglePanel(listPanel));
    }

    private void OnEnable()
    {
        TrainManager.TrainCreated += OnTrainCreated;
        TrainManager.TrainRemoved += OnTrainRemoved;
        TrainManager.TrainSelected += OnTrainSelected;
        TrainManager.TrainDeselected += OnTrainDeselected;
    }

    private void OnDisable()
    {
        TrainManager.TrainCreated -= OnTrainCreated;
        TrainManager.TrainRemoved -= OnTrainRemoved;
        TrainManager.TrainSelected -= OnTrainSelected;
        TrainManager.TrainDeselected -= OnTrainDeselected;
    }

    private void OnTrainCreated(Train train, RailLine line)
    {
        if (listEntryPrefab == null || listContent == null || train == null)
            return;

        GameObject entryObj = Instantiate(listEntryPrefab, listContent);
        UITrainLineEntry entry = entryObj.GetComponent<UITrainLineEntry>();
        if (entry == null)
        {
            Debug.LogError("No UITraiLineEntry on prefab");
            return;
        }

        entry.Init(train, line);
        entry.OnSelectClicked += HandleSelectClicked;
        entry.OnDeleteClicked += HandleDeleteClicked;
        entry.OnSpeedClicked += HandleSpeedClicked;
        entry.OnCapacityClicked += HandleCapacityClicked;

        entries[train] = entry;
    }

    private void OnTrainRemoved(Train train)
    {
        if (train == null)
            return;

        if (entries.TryGetValue(train, out UITrainLineEntry entry))
        {
            entry.OnSelectClicked -= HandleSelectClicked;
            entry.OnDeleteClicked -= HandleDeleteClicked;
            entry.OnSpeedClicked -= HandleSpeedClicked;
            entry.OnCapacityClicked -= HandleCapacityClicked;
            Destroy(entry.gameObject);
            entries.Remove(train);
        }
    }

    private void OnTrainSelected(Train train)
    {
        if (entries.TryGetValue(train, out UITrainLineEntry entry))
            entry.SetSelected(true);
    }

    private void OnTrainDeselected(Train train)
    {
        if (entries.TryGetValue(train, out UITrainLineEntry entry))
            entry.SetSelected(false);
    }

    private void HandleSelectClicked(UITrainLineEntry entry)
    {
        if (entry == null || entry.ReferenceTrain == null)
            return;

        TrainManager.Instance.ToggleSelection(entry.ReferenceTrain);
    }

    private void HandleDeleteClicked(UITrainLineEntry entry)
    {
        if (entry == null || entry.ReferenceTrain == null)
            return;

        TrainManager.Instance.RemoveTrain(entry.ReferenceTrain);
    }

    private void HandleSpeedClicked(UITrainLineEntry entry)
    {
        Train train = entry.ReferenceTrain;
        if (train == null)
            return;

        train.SetSpeedLevel(train.SpeedLevel + 1);
        entry.UpdateSpeedText();
    }

    private void HandleCapacityClicked(UITrainLineEntry entry)
    {
        Train train = entry.ReferenceTrain;
        if (train == null)
            return;

        train.TryAddWagon();
        entry.UpdateCapacityText();
    }

    private void TogglePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }
}
