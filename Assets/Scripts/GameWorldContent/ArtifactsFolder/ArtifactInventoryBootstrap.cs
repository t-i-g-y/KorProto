using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ArtifactInventoryBootstrap
{
    private static ArtifactInventoryUI inventoryInstance;

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
        if (inventoryInstance != null || Object.FindAnyObjectByType<ArtifactInventoryUI>() != null)
            return;

        if (scene.name == "MainMenu")
            return;

        CreateInventory();
    }

    private static void CreateInventory()
    {
        GameObject root = new("ArtifactInventoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ArtifactInventoryUI));
        Object.DontDestroyOnLoad(root);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1001;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Button toggleButton = CreateButton(root.transform, "ArtifactInventoryButton", "Артефакты", new Vector2(150f, 30f));
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 1f);
        toggleRect.anchorMax = new Vector2(1f, 1f);
        toggleRect.pivot = new Vector2(1f, 1f);
        toggleRect.anchoredPosition = new Vector2(-250f, -265f);

        GameObject panel = CreatePanel(root.transform);
        Button closeButton = CreateCloseButton(panel.transform);
        Transform grid = CreateGrid(panel.transform, out TMP_Text emptyText);
        GameObject detailPanel = CreateDetailPanel(panel.transform, out Image detailIcon, out TMP_Text detailTitle, out TMP_Text detailStory);

        inventoryInstance = root.GetComponent<ArtifactInventoryUI>();
        inventoryInstance.Configure(toggleButton, closeButton, panel, grid, detailPanel, detailIcon, detailTitle, detailStory, emptyText);
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
        text.fontSize = 22f;
        text.text = label;

        return buttonObject.GetComponent<Button>();
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new("ArtifactInventoryPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(980f, 760f);
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = new Color32(239, 240, 234, 248);

        GameObject titleObject = new("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(28f, -74f);
        titleRect.offsetMax = new Vector2(-92f, -24f);

        TMP_Text title = titleObject.GetComponent<TMP_Text>();
        title.color = Color.black;
        title.fontSize = 34f;
        title.fontStyle = FontStyles.Bold;
        title.text = "Артефакты";

        return panel;
    }

    private static Button CreateCloseButton(Transform parent)
    {
        Button closeButton = CreateButton(parent, "CloseArtifactInventoryButton", "X", new Vector2(48f, 48f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-24f, -24f);

        return closeButton;
    }

    private static Transform CreateGrid(Transform panel, out TMP_Text emptyText)
    {
        GameObject scrollObject = new("ArtifactScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(panel, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0.31f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(36f, 24f);
        scrollRectTransform.offsetMax = new Vector2(-28f, -96f);

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

        GameObject content = new("Grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();
        layout.cellSize = new Vector2(104f, 104f);
        layout.spacing = new Vector2(14f, 14f);
        layout.padding = new RectOffset(14, 14, 14, 14);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = 6;

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
        emptyText.text = "Артефактов пока нет";

        return content.transform;
    }

    private static GameObject CreateDetailPanel(Transform parent, out Image detailIcon, out TMP_Text detailTitle, out TMP_Text detailStory)
    {
        GameObject panel = new("ArtifactDetail", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, 0.29f);
        rect.offsetMin = new Vector2(36f, 28f);
        rect.offsetMax = new Vector2(-28f, -12f);

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

        GameObject titleObject = new("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(116f, -72f);
        titleRect.offsetMax = new Vector2(-18f, -16f);

        detailTitle = titleObject.GetComponent<TMP_Text>();
        detailTitle.color = Color.black;
        detailTitle.fontSize = 24f;
        detailTitle.fontStyle = FontStyles.Bold;
        detailTitle.textWrappingMode = TextWrappingModes.Normal;

        GameObject storyObject = new("Story", typeof(RectTransform), typeof(TextMeshProUGUI));
        storyObject.transform.SetParent(panel.transform, false);

        RectTransform storyRect = storyObject.GetComponent<RectTransform>();
        storyRect.anchorMin = Vector2.zero;
        storyRect.anchorMax = Vector2.one;
        storyRect.offsetMin = new Vector2(18f, 18f);
        storyRect.offsetMax = new Vector2(-18f, -106f);

        detailStory = storyObject.GetComponent<TMP_Text>();
        detailStory.color = Color.black;
        detailStory.fontSize = 18f;
        detailStory.textWrappingMode = TextWrappingModes.Normal;
        detailStory.alignment = TextAlignmentOptions.TopLeft;

        return panel;
    }
}
