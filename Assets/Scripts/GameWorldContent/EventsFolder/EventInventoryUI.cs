using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventInventoryUI : MonoBehaviour
{
    [SerializeField] private EventManager eventManager;
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TMP_Text emptyText;

    private readonly List<GameObject> spawnedEntries = new();
    private EventManager boundManager;

    private void Awake()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        TryBindManager();
        Rebuild();
    }

    private void OnDisable()
    {
        if (boundManager != null)
        {
            boundManager.EventRecorded -= HandleEventRecorded;
            boundManager = null;
        }
    }

    private void Update()
    {
        if (eventManager == null)
            TryBindManager();
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);

        if (panelRoot.activeSelf)
            Rebuild();
    }

    public void Rebuild()
    {
        Clear();

        IReadOnlyList<EventHistoryEntry> history = boundManager != null
            ? boundManager.History
            : null;

        bool hasEntries = history != null && history.Count > 0;

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasEntries);

        if (!hasEntries || contentRoot == null)
            return;

        for (int i = history.Count - 1; i >= 0; i--)
            SpawnEntry(history[i]);
    }

    private void TryBindManager()
    {
        EventManager manager = eventManager != null ? eventManager : EventManager.Instance;
        if (manager == null)
            return;

        if (boundManager == manager)
            return;

        if (boundManager != null)
            boundManager.EventRecorded -= HandleEventRecorded;

        boundManager = manager;
        boundManager.EventRecorded += HandleEventRecorded;
        Rebuild();
    }

    private void HandleEventRecorded(EventHistoryEntry entry)
    {
        if (panelRoot != null && panelRoot.activeSelf)
            Rebuild();

        if (emptyText != null)
            emptyText.gameObject.SetActive(boundManager == null || boundManager.History.Count == 0);
    }

    private void SpawnEntry(EventHistoryEntry entry)
    {
        GameObject entryObject = entryPrefab != null
            ? Instantiate(entryPrefab, contentRoot)
            : CreateDefaultEntryObject();

        if (entryObject == null)
            return;

        EventInventoryEntryUI entryUI = entryObject.GetComponent<EventInventoryEntryUI>();
        if (entryUI != null)
            entryUI.Set(entry);
        else
            SetFallbackText(entryObject, entry);

        spawnedEntries.Add(entryObject);
    }

    private GameObject CreateDefaultEntryObject()
    {
        if (contentRoot == null)
            return null;

        GameObject entryObject = new("EventHistoryEntry", typeof(RectTransform), typeof(TextMeshProUGUI));
        entryObject.transform.SetParent(contentRoot, false);
        return entryObject;
    }

    private static void SetFallbackText(GameObject entryObject, EventHistoryEntry entry)
    {
        TMP_Text text = entryObject.GetComponent<TMP_Text>();
        if (text == null)
            return;

        text.textWrappingMode = TextWrappingModes.Normal;
        text.color = Color.black;
        text.text = $"День {entry.day}, {entry.hour:00}:00 - {entry.title}\n{entry.consequenceTitle}: {entry.consequenceEffects}";
    }

    private void Clear()
    {
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            if (spawnedEntries[i] != null)
                Destroy(spawnedEntries[i]);
        }

        spawnedEntries.Clear();
    }
}
