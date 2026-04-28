using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private TMP_Text emptyText;

    private readonly List<GameObject> spawnedEntries = new();
    private QuestManager boundManager;

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
            boundManager.QuestActivated -= HandleQuestChanged;
            boundManager.QuestProgressChanged -= HandleQuestChanged;
            boundManager.QuestCompleted -= HandleQuestChanged;
            boundManager.QuestListChanged -= HandleQuestListChanged;
            boundManager = null;
        }
    }

    private void Update()
    {
        if (questManager == null)
            TryBindManager();
    }

    public void Configure(Button button, GameObject panel, Transform content, TMP_Text emptyLabel, GameObject prefab = null)
    {
        toggleButton = button;
        panelRoot = panel;
        contentRoot = content;
        emptyText = emptyLabel;
        entryPrefab = prefab;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (panelRoot != null)
            panelRoot.SetActive(false);
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

        bool hasEntries = boundManager != null &&
            (boundManager.ActiveQuests.Count > 0 || boundManager.CompletedQuests.Count > 0);

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasEntries);

        if (!hasEntries || contentRoot == null)
            return;

        foreach (QuestRuntime quest in boundManager.ActiveQuests)
            SpawnEntry(quest);

        for (int i = boundManager.CompletedQuests.Count - 1; i >= 0; i--)
            SpawnEntry(boundManager.CompletedQuests[i]);
    }

    private void TryBindManager()
    {
        QuestManager manager = questManager != null ? questManager : QuestManager.Instance;
        if (manager == null || boundManager == manager)
            return;

        if (boundManager != null)
        {
            boundManager.QuestActivated -= HandleQuestChanged;
            boundManager.QuestProgressChanged -= HandleQuestChanged;
            boundManager.QuestCompleted -= HandleQuestChanged;
            boundManager.QuestListChanged -= HandleQuestListChanged;
        }

        boundManager = manager;
        boundManager.QuestActivated += HandleQuestChanged;
        boundManager.QuestProgressChanged += HandleQuestChanged;
        boundManager.QuestCompleted += HandleQuestChanged;
        boundManager.QuestListChanged += HandleQuestListChanged;
        Rebuild();
    }

    private void HandleQuestChanged(QuestRuntime quest)
    {
        if (panelRoot != null && panelRoot.activeSelf)
            Rebuild();

        UpdateEmptyState();
    }

    private void HandleQuestListChanged()
    {
        if (panelRoot != null && panelRoot.activeSelf)
            Rebuild();

        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        if (emptyText == null)
            return;

        bool hasEntries = boundManager != null &&
            (boundManager.ActiveQuests.Count > 0 || boundManager.CompletedQuests.Count > 0);
        emptyText.gameObject.SetActive(!hasEntries);
    }

    private void SpawnEntry(QuestRuntime quest)
    {
        GameObject entryObject = entryPrefab != null
            ? Instantiate(entryPrefab, contentRoot)
            : CreateDefaultEntryObject();

        if (entryObject == null)
            return;

        QuestJournalEntryUI entryUI = entryObject.GetComponent<QuestJournalEntryUI>();
        if (entryUI != null)
            entryUI.Set(quest);
        else
            SetFallbackText(entryObject, quest);

        spawnedEntries.Add(entryObject);
    }

    private GameObject CreateDefaultEntryObject()
    {
        if (contentRoot == null)
            return null;

        GameObject entryObject = new("QuestJournalEntry", typeof(RectTransform), typeof(TextMeshProUGUI));
        entryObject.transform.SetParent(contentRoot, false);

        RectTransform rect = entryObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 140f);

        LayoutElement layout = entryObject.AddComponent<LayoutElement>();
        layout.minHeight = 140f;
        layout.preferredHeight = 140f;

        return entryObject;
    }

    private static void SetFallbackText(GameObject entryObject, QuestRuntime quest)
    {
        TMP_Text text = entryObject.GetComponent<TMP_Text>();
        if (text == null || quest == null)
            return;

        text.textWrappingMode = TextWrappingModes.Normal;
        text.color = Color.black;
        text.fontSize = 18f;
        text.text = $"{quest.title} ({(quest.Status == QuestStatus.Completed ? "выполнено" : "активно")})\n" +
                    $"{quest.description}\n" +
                    $"Условия: {quest.activationCondition}. Цель: {quest.objectiveSummary}\n" +
                    $"Награда: {quest.rewardSummary}\n" +
                    $"Прогресс: {quest.progress}/{quest.targetValue}";
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
