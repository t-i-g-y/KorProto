using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PathJournalBootstrap
{
    private const int PanelSortingOrder = 1100;
    private static GameObject journalRoot;
    private static GameObject panelRoot;
    private static GameObject notificationBadge;
    private static GameObject questsPanel;
    private static GameObject eventsPanel;
    private static GameObject artifactsPanel;
    private static QuestJournalUI questJournal;
    private static EventInventoryUI eventInventory;
    private static ArtifactInventoryUI artifactInventory;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryCreateForScene(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryCreateForScene(scene);
    }

    private static void TryCreateForScene(Scene scene)
    {
        if (journalRoot != null)
            return;

        if (scene.name == "MainMenu")
            return;

        DestroySceneJournals();
        CreateJournal();
    }

    private static void DestroySceneJournals()
    {
        DestroyObjects(Object.FindObjectsByType<QuestJournalUI>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        DestroyObjects(Object.FindObjectsByType<EventInventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        DestroyObjects(Object.FindObjectsByType<ArtifactInventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private static void DestroyObjects(MonoBehaviour[] behaviours)
    {
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour != null)
                Object.Destroy(behaviour.gameObject);
        }
    }

    private static void CreateJournal()
    {
        journalRoot = new GameObject("PathJournalCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Object.DontDestroyOnLoad(journalRoot);

        Canvas canvas = journalRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -10;

        CanvasScaler scaler = journalRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Button toggleButton = CreateButton(journalRoot.transform, "PathJournalButton", "", new Vector2(300f, 70f));
        Image buttonImage = toggleButton.GetComponent<Image>();
        buttonImage.sprite = LoadSpriteResource("Artifacts/free-icon-open-book-167755");
        buttonImage.preserveAspect = true;
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 1f);
        toggleRect.anchorMax = new Vector2(1f, 1f);
        toggleRect.pivot = new Vector2(1f, 1f);
        toggleRect.anchoredPosition = new Vector2(-100f, -250f);

        notificationBadge = CreateNotificationBadge(toggleButton.transform);
        panelRoot = CreatePanel(journalRoot.transform);
        Button closeButton = CreateCloseButton(panelRoot.transform);
        Transform tabsRoot = CreateTabsRoot(panelRoot.transform);

        Button questsTab = CreateButton(tabsRoot, "QuestsTabButton", "Квесты", new Vector2(160f, 42f));
        Button eventsTab = CreateButton(tabsRoot, "EventsTabButton", "События", new Vector2(160f, 42f));
        Button artifactsTab = CreateButton(tabsRoot, "ArtifactsTabButton", "Артефакты", new Vector2(160f, 42f));

        Transform contentRoot = CreateContentRoot(panelRoot.transform);
        CreateQuestTab(contentRoot);
        CreateEventTab(contentRoot);
        CreateArtifactTab(contentRoot);

        toggleButton.onClick.AddListener(ToggleJournal);
        closeButton.onClick.AddListener(CloseJournal);
        questsTab.onClick.AddListener(ShowQuests);
        eventsTab.onClick.AddListener(ShowEvents);
        artifactsTab.onClick.AddListener(ShowArtifacts);

        CloseJournal();
    }

    private static void ToggleJournal()
    {
        if (panelRoot == null)
            return;

        if (panelRoot.activeSelf)
            CloseJournal();
        else
            OpenJournal();
    }

    private static void OpenJournal()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling();

        if (notificationBadge != null)
            notificationBadge.SetActive(false);

        ShowQuests();
    }

    private static void CloseJournal()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        SetTabVisible(questsPanel, false);
        SetTabVisible(eventsPanel, false);
        SetTabVisible(artifactsPanel, false);
    }

    private static void ShowQuests()
    {
        SetTabVisible(questsPanel, true);
        SetTabVisible(eventsPanel, false);
        SetTabVisible(artifactsPanel, false);
        questJournal?.Rebuild();
    }

    private static void ShowEvents()
    {
        SetTabVisible(questsPanel, false);
        SetTabVisible(eventsPanel, true);
        SetTabVisible(artifactsPanel, false);
        eventInventory?.Rebuild();
    }

    private static void ShowArtifacts()
    {
        SetTabVisible(questsPanel, false);
        SetTabVisible(eventsPanel, false);
        SetTabVisible(artifactsPanel, true);
        artifactInventory?.Rebuild();
    }

    private static void SetTabVisible(GameObject tab, bool visible)
    {
        if (tab != null)
            tab.SetActive(visible);
    }

    private static void CreateQuestTab(Transform parent)
    {
        questsPanel = CreateTabPanel(parent, "QuestJournalPanel");
        Transform content = CreateScrollContent(questsPanel.transform, "QuestScroll", "Нет активных заданий", out TMP_Text emptyText);

        questJournal = journalRoot.AddComponent<QuestJournalUI>();
        questJournal.Configure(null, null, notificationBadge, questsPanel, content, emptyText);
    }

    private static void CreateEventTab(Transform parent)
    {
        eventsPanel = CreateTabPanel(parent, "EventInventoryPanel");
        Transform content = CreateScrollContent(eventsPanel.transform, "EventScroll", "Нет записанных событий", out TMP_Text emptyText);

        eventInventory = journalRoot.AddComponent<EventInventoryUI>();
        eventInventory.Configure(null, null, eventsPanel, content, emptyText);
    }

    private static void CreateArtifactTab(Transform parent)
    {
        artifactsPanel = CreateTabPanel(parent, "ArtifactInventoryPanel");
        Transform grid = CreateArtifactGrid(artifactsPanel.transform);
        GameObject detailPanel = CreateArtifactDetailPanel(artifactsPanel.transform, out Image detailIcon, out TMP_Text detailTitle, out TMP_Text detailStory);

        artifactInventory = journalRoot.AddComponent<ArtifactInventoryUI>();
        artifactInventory.Configure(null, null, artifactsPanel, grid, detailPanel, detailIcon, detailTitle, detailStory);
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new("PathJournalPanel", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = new Color32(245, 246, 242, 255);

        Canvas canvas = panel.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = PanelSortingOrder;

        TMP_Text title = CreateText(panel.transform, "Title", "Журнал путейца", 42f, FontStyles.Bold, TextAlignmentOptions.Left);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(48f, -92f);
        titleRect.offsetMax = new Vector2(-160f, -28f);

        return panel;
    }

    private static Transform CreateTabsRoot(Transform parent)
    {
        GameObject tabs = new("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabs.transform.SetParent(parent, false);

        RectTransform rect = tabs.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(48f, -150f);
        rect.offsetMax = new Vector2(-48f, -104f);

        HorizontalLayoutGroup layout = tabs.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;

        return tabs.transform;
    }

    private static Transform CreateContentRoot(Transform parent)
    {
        GameObject content = new("JournalContent", typeof(RectTransform));
        content.transform.SetParent(parent, false);

        RectTransform rect = content.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(48f, 48f);
        rect.offsetMax = new Vector2(-48f, -170f);

        return content.transform;
    }

    private static GameObject CreateTabPanel(Transform parent, string name)
    {
        GameObject panel = new(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return panel;
    }

    private static Transform CreateScrollContent(Transform parent, string name, string emptyMessage, out TMP_Text emptyText)
    {
        GameObject scrollObject = new(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        Image scrollImage = scrollObject.GetComponent<Image>();
        scrollImage.color = new Color32(255, 255, 255, 210);

        GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObject.transform, false);

        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(12f, 12f);
        viewportRect.offsetMax = new Vector2(-12f, -12f);

        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;

        GameObject emptyObject = new("EmptyText", typeof(RectTransform), typeof(TextMeshProUGUI));
        emptyObject.transform.SetParent(viewport.transform, false);

        RectTransform emptyRect = emptyObject.GetComponent<RectTransform>();
        emptyRect.anchorMin = Vector2.zero;
        emptyRect.anchorMax = Vector2.one;
        emptyRect.offsetMin = Vector2.zero;
        emptyRect.offsetMax = Vector2.zero;

        emptyText = emptyObject.GetComponent<TMP_Text>();
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyText.color = Color.black;
        emptyText.fontSize = 20f;
        emptyText.text = emptyMessage;

        return content.transform;
    }

    private static Transform CreateArtifactGrid(Transform parent)
    {
        GameObject scrollObject = new("ArtifactScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(parent, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0.31f);
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;

        Image scrollImage = scrollObject.GetComponent<Image>();
        scrollImage.color = new Color32(255, 255, 255, 220);

        GameObject viewport = new("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObject.transform, false);

        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(12f, 12f);
        viewportRect.offsetMax = new Vector2(-12f, -12f);

        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = Color.white;
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject grid = new("Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        grid.transform.SetParent(viewport.transform, false);

        RectTransform gridRect = grid.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 1f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0.5f, 1f);
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

        GridLayoutGroup layout = grid.GetComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(104f, 104f);
        layout.spacing = new Vector2(14f, 14f);
        layout.padding = new RectOffset(14, 14, 14, 14);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 6;

        ContentSizeFitter fitter = grid.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = gridRect;
        scrollRect.horizontal = false;

        return grid.transform;
    }

    private static GameObject CreateArtifactDetailPanel(Transform parent, out Image detailIcon, out TMP_Text detailTitle, out TMP_Text detailStory)
    {
        GameObject panel = new("ArtifactDetail", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, 0.29f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = new Vector2(0f, -12f);

        Image image = panel.GetComponent<Image>();
        image.color = new Color32(255, 255, 255, 225);

        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(panel.transform, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 1f);
        iconRect.anchorMax = new Vector2(0f, 1f);
        iconRect.pivot = new Vector2(0f, 1f);
        iconRect.anchoredPosition = new Vector2(18f, -18f);
        iconRect.sizeDelta = new Vector2(82f, 82f);

        detailIcon = iconObject.GetComponent<Image>();
        detailIcon.preserveAspect = true;

        detailTitle = CreateText(panel.transform, "Title", string.Empty, 24f, FontStyles.Bold, TextAlignmentOptions.Left);
        RectTransform titleRect = detailTitle.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(116f, -72f);
        titleRect.offsetMax = new Vector2(-18f, -16f);

        detailStory = CreateText(panel.transform, "Story", string.Empty, 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform storyRect = detailStory.GetComponent<RectTransform>();
        storyRect.anchorMin = Vector2.zero;
        storyRect.anchorMax = Vector2.one;
        storyRect.offsetMin = new Vector2(18f, 18f);
        storyRect.offsetMax = new Vector2(-18f, -106f);

        return panel;
    }

    private static Button CreateCloseButton(Transform parent)
    {
        Button closeButton = CreateButton(parent, "ClosePathJournalButton", "X", new Vector2(64f, 64f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-36f, -28f);

        return closeButton;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size)
    {
        GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = Color.white;

        GameObject textObject = new("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        text.fontSize = 20f;
        text.text = label;

        return buttonObject.GetComponent<Button>();
    }

    private static Sprite LoadSpriteResource(string path)
    {
        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
            return sprite;

        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        return sprites.Length > 0 ? sprites[0] : null;
    }

    private static GameObject CreateNotificationBadge(Transform parent)
    {
        GameObject badge = new("JournalNotificationBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(parent, false);

        RectTransform rect = badge.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(8f, 8f);
        rect.sizeDelta = new Vector2(28f, 28f);

        Image image = badge.GetComponent<Image>();
        image.color = new Color32(220, 38, 38, 255);

        TMP_Text text = CreateText(badge.transform, "Text", "!", 20f, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.color = Color.white;

        badge.SetActive(false);
        return badge;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.color = Color.black;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.text = value;

        return text;
    }
}
