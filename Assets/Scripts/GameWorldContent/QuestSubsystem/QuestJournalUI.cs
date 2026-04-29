using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject notificationRoot;
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

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        SetNotificationVisible(false);

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
            boundManager.QuestActivated -= HandleQuestActivated;
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

    public void Configure(Button button, Button close, GameObject notification, GameObject panel, Transform content, TMP_Text emptyLabel, GameObject prefab = null)
    {
        toggleButton = button;
        closeButton = close;
        notificationRoot = notification;
        panelRoot = panel;
        contentRoot = content;
        emptyText = emptyLabel;
        entryPrefab = prefab;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        SetNotificationVisible(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);

        if (panelRoot.activeSelf)
        {
            SetNotificationVisible(false);
            Rebuild();
        }
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
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

        bool hasSpawnedEntry = false;

        foreach (QuestRuntime quest in boundManager.ActiveQuests)
        {
            SpawnEntryWithSeparator(quest, hasSpawnedEntry);
            hasSpawnedEntry = true;
        }

        for (int i = boundManager.CompletedQuests.Count - 1; i >= 0; i--)
        {
            SpawnEntryWithSeparator(boundManager.CompletedQuests[i], hasSpawnedEntry);
            hasSpawnedEntry = true;
        }
    }

    private void TryBindManager()
    {
        QuestManager manager = questManager != null ? questManager : QuestManager.Instance;
        if (manager == null || boundManager == manager)
            return;

        if (boundManager != null)
        {
            boundManager.QuestActivated -= HandleQuestActivated;
            boundManager.QuestProgressChanged -= HandleQuestChanged;
            boundManager.QuestCompleted -= HandleQuestChanged;
            boundManager.QuestListChanged -= HandleQuestListChanged;
        }

        boundManager = manager;
        boundManager.QuestActivated += HandleQuestActivated;
        boundManager.QuestProgressChanged += HandleQuestChanged;
        boundManager.QuestCompleted += HandleQuestChanged;
        boundManager.QuestListChanged += HandleQuestListChanged;
        Rebuild();
    }

    private void HandleQuestActivated(QuestRuntime quest)
    {
        if (panelRoot != null && panelRoot.activeSelf)
            Rebuild();
        else
            SetNotificationVisible(true);

        UpdateEmptyState();
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

    private void SetNotificationVisible(bool visible)
    {
        if (notificationRoot != null)
            notificationRoot.SetActive(visible);
    }

    private void SpawnEntryWithSeparator(QuestRuntime quest, bool addSeparator)
    {
        if (addSeparator)
            SpawnSeparator();

        SpawnEntry(quest);
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

    private void SpawnSeparator()
    {
        if (contentRoot == null)
            return;

        GameObject separator = new("QuestJournalSeparator", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        separator.transform.SetParent(contentRoot, false);

        Image image = separator.GetComponent<Image>();
        image.color = new Color32(40, 40, 40, 90);

        LayoutElement layout = separator.GetComponent<LayoutElement>();
        layout.minHeight = 2f;
        layout.preferredHeight = 2f;

        spawnedEntries.Add(separator);
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
        text.richText = true;
        text.color = Color.black;
        text.fontSize = 18f;
        string progressColor = quest.IsComplete ? "#188038" : "#BE2828";
        text.text = $"{quest.title} ({(quest.Status == QuestStatus.Completed ? "выполнено" : "активно")})\n" +
                    $"{quest.description}\n" +
                    $"Условия: {quest.activationCondition}. Цель: {quest.objectiveSummary}\n" +
                    $"Награда: {quest.rewardSummary}\n" +
                    $"<color={progressColor}>Прогресс: {quest.progress}/{quest.targetValue}</color>";
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
