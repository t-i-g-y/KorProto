using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EventInventoryBootstrap
{
    private const int PanelSortingOrder = 1090;
    private static EventInventoryUI inventoryInstance;

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
        if (inventoryInstance != null)
            return;

        if (scene.name == "MainMenu")
            return;

        DestroySceneInventories();
        CreateInventory();
    }

    private static void DestroySceneInventories()
    {
        EventInventoryUI[] inventories = Object.FindObjectsByType<EventInventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EventInventoryUI inventory in inventories)
        {
            if (inventory != null)
                Object.Destroy(inventory.gameObject);
        }
    }

    private static void CreateInventory()
    {
        GameObject root = new("EventInventoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(EventInventoryUI));
        Object.DontDestroyOnLoad(root);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 7;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Button toggleButton = CreateButton(root.transform, "EventInventoryButton", "События", new Vector2(120f, 30f));
        RectTransform toggleRect = toggleButton.GetComponent<RectTransform>();
        toggleRect.anchorMin = new Vector2(1f, 1f);
        toggleRect.anchorMax = new Vector2(1f, 1f);
        toggleRect.pivot = new Vector2(1f, 1f);
        toggleRect.anchoredPosition = new Vector2(-250f, -265f);

        GameObject panel = CreatePanel(root.transform);
        Transform content = CreateScrollContent(panel.transform, out TMP_Text emptyText);
        Button closeButton = CreateCloseButton(panel.transform);

        inventoryInstance = root.GetComponent<EventInventoryUI>();
        inventoryInstance.Configure(toggleButton, closeButton, panel, content, emptyText);
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size)
    {
        GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();

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

        return button;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new("EventInventoryPanel", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = new Color32(245, 246, 242, 255);

        Canvas canvas = panel.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = PanelSortingOrder;

        GameObject titleObject = new("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(panel.transform, false);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.offsetMin = new Vector2(48f, -96f);
        titleRect.offsetMax = new Vector2(-128f, -28f);

        TMP_Text title = titleObject.GetComponent<TMP_Text>();
        title.color = Color.black;
        title.fontSize = 42f;
        title.fontStyle = FontStyles.Bold;
        title.text = "Журнал событий";

        return panel;
    }

    private static Button CreateCloseButton(Transform parent)
    {
        Button closeButton = CreateButton(parent, "CloseEventInventoryButton", "X", new Vector2(64f, 64f));
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-36f, -28f);

        return closeButton;
    }

    private static Transform CreateScrollContent(Transform panel, out TMP_Text emptyText)
    {
        GameObject scrollObject = new("Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(panel, false);

        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(48f, 48f);
        scrollRectTransform.offsetMax = new Vector2(-48f, -120f);

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
        emptyText.text = "Нет записанных событий";

        return content.transform;
    }
}
