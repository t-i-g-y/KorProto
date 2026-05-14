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
            panelRoot.transform.SetAsLastSibling();
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

        IReadOnlyList<QuestRuntime> quests = boundManager != null ? boundManager.BuildJournalQuests() : null;
        bool hasEntries = quests != null && quests.Count > 0;

        if (emptyText != null)
            emptyText.gameObject.SetActive(!hasEntries);

        if (!hasEntries || contentRoot == null)
            return;

        bool hasSpawnedEntry = false;

        foreach (QuestRuntime quest in quests)
        {
            SpawnEntryWithSeparator(quest, hasSpawnedEntry);
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

        IReadOnlyList<QuestRuntime> quests = boundManager != null ? boundManager.BuildJournalQuests() : null;
        bool hasEntries = quests != null && quests.Count > 0;
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

        GameObject entryObject = new("QuestJournalEntry", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement), typeof(QuestJournalEntryUI));
        entryObject.transform.SetParent(contentRoot, false);

        RectTransform rect = entryObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 190f);

        Image image = entryObject.GetComponent<Image>();
        image.color = new Color32(255, 255, 255, 225);

        VerticalLayoutGroup layoutGroup = entryObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(14, 14, 10, 10);
        layoutGroup.spacing = 5f;
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;

        LayoutElement layout = entryObject.GetComponent<LayoutElement>();
        layout.minHeight = 190f;
        layout.preferredHeight = 190f;

        TMP_Text titleText = CreateEntryText(entryObject.transform, "Title", 20f, FontStyles.Bold);
        TMP_Text statusText = CreateEntryText(entryObject.transform, "Status", 16f, FontStyles.Normal);
        TMP_Text descriptionText = CreateEntryText(entryObject.transform, "Description", 16f, FontStyles.Normal);
        TMP_Text conditionText = CreateEntryText(entryObject.transform, "Condition", 15f, FontStyles.Normal);
        TMP_Text rewardText = CreateEntryText(entryObject.transform, "Reward", 15f, FontStyles.Normal);
        Slider progressSlider = CreateProgressSlider(entryObject.transform);
        TMP_Text progressText = CreateEntryText(entryObject.transform, "Progress", 15f, FontStyles.Bold);

        QuestJournalEntryUI entryUI = entryObject.GetComponent<QuestJournalEntryUI>();
        entryUI.Configure(titleText, statusText, descriptionText, conditionText, rewardText, progressText, progressSlider);

        return entryObject;
    }

    private static TMP_Text CreateEntryText(Transform parent, string name, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.color = Color.black;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.textWrappingMode = TextWrappingModes.Normal;

        return text;
    }

    private static Slider CreateProgressSlider(Transform parent)
    {
        GameObject sliderObject = new("ProgressSlider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
        sliderObject.transform.SetParent(parent, false);

        LayoutElement layout = sliderObject.GetComponent<LayoutElement>();
        layout.minHeight = 18f;
        layout.preferredHeight = 18f;

        GameObject backgroundObject = new("Background", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(sliderObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundObject.GetComponent<Image>().color = new Color32(220, 224, 219, 255);

        GameObject fillAreaObject = new("Fill Area", typeof(RectTransform));
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillObject = new("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillObject.GetComponent<Image>().color = new Color32(24, 128, 56, 255);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.targetGraphic = backgroundObject.GetComponent<Image>();
        slider.fillRect = fillRect;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        return slider;
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
        string status = quest.Status switch
        {
            QuestStatus.Completed => "выполнено",
            QuestStatus.Active => "активно",
            _ => "неактивно"
        };
        text.text = $"{quest.title} ({status})\n" +
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
