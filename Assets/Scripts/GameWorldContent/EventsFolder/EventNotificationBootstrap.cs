using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class EventNotificationBootstrap
{
    private const int PanelSortingOrder = 1200;
    private static EventNotificationUI notificationInstance;

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
        if (notificationInstance != null)
            return;

        if (scene.name == "MainMenu")
            return;

        DestroySceneNotifications();
        CreateNotification();
    }

    private static void DestroySceneNotifications()
    {
        EventNotificationUI[] notifications = Object.FindObjectsByType<EventNotificationUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EventNotificationUI notification in notifications)
        {
            if (notification != null)
                Object.Destroy(notification.gameObject);
        }
    }

    private static void CreateNotification()
    {
        GameObject canvasObject = new("EventNotificationCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Object.DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = PanelSortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject blocker = CreateInputBlocker(canvasObject.transform);
        GameObject panel = CreatePanel(canvasObject.transform);
        TMP_Text title = CreateText(panel.transform, "Title", 34f, FontStyles.Bold, TextAlignmentOptions.Center);
        TMP_Text description = CreateText(panel.transform, "Description", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        TMP_Text consequence = CreateText(panel.transform, "Consequence", 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Transform options = CreateOptionsContainer(panel.transform);
        Button closeButton = CreateButton(panel.transform, "CloseEventNotificationButton", "Закрыть", new Vector2(180f, 48f));

        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(32f, -84f);
        titleRect.offsetMax = new Vector2(-32f, -24f);

        RectTransform descriptionRect = description.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0f, 0.6f);
        descriptionRect.anchorMax = new Vector2(1f, 1f);
        descriptionRect.offsetMin = new Vector2(36f, 8f);
        descriptionRect.offsetMax = new Vector2(-36f, -96f);

        RectTransform consequenceRect = consequence.GetComponent<RectTransform>();
        consequenceRect.anchorMin = new Vector2(0f, 0.42f);
        consequenceRect.anchorMax = new Vector2(1f, 0.6f);
        consequenceRect.offsetMin = new Vector2(36f, 8f);
        consequenceRect.offsetMax = new Vector2(-36f, -8f);

        RectTransform optionsRect = options.GetComponent<RectTransform>();
        optionsRect.anchorMin = new Vector2(0f, 0.1f);
        optionsRect.anchorMax = new Vector2(1f, 0.42f);
        optionsRect.offsetMin = new Vector2(36f, 12f);
        optionsRect.offsetMax = new Vector2(-36f, -8f);

        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.pivot = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0f, 28f);

        notificationInstance = canvasObject.AddComponent<EventNotificationUI>();
        notificationInstance.Configure(panel, title, description, consequence, options, closeButton, blocker);
    }

    private static GameObject CreateInputBlocker(Transform parent)
    {
        GameObject blocker = new("EventInputBlocker", typeof(RectTransform), typeof(Image));
        blocker.transform.SetParent(parent, false);

        RectTransform rect = blocker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = blocker.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = true;

        return blocker;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new("EventNotificationPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(760f, 500f);
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = new Color32(245, 246, 242, 255);

        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.color = Color.black;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;

        return text;
    }

    private static Transform CreateOptionsContainer(Transform parent)
    {
        GameObject container = new("Options", typeof(RectTransform), typeof(VerticalLayoutGroup));
        container.transform.SetParent(parent, false);

        VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        return container.transform;
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
}
