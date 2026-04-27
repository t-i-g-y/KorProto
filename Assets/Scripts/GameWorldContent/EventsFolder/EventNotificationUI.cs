using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventNotificationUI : MonoBehaviour
{
    [SerializeField] private EventManager eventManager;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text consequenceText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private Button closeButton;

    private readonly List<Button> spawnedOptionButtons = new();
    private EventManager boundManager;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        Hide();
    }

    private void OnEnable()
    {
        TryBindManager();
    }

    private void OnDisable()
    {
        if (boundManager != null)
        {
            boundManager.EventActivated -= Show;
            boundManager.EventResolved -= ShowResolved;
            boundManager = null;
        }
    }

    private void Update()
    {
        if (eventManager == null)
            TryBindManager();
    }

    private void TryBindManager()
    {
        EventManager manager = eventManager != null ? eventManager : EventManager.Instance;
        if (manager == null)
            return;

        if (boundManager == manager)
            return;

        if (boundManager != null)
        {
            boundManager.EventActivated -= Show;
            boundManager.EventResolved -= ShowResolved;
        }

        boundManager = manager;
        boundManager.EventActivated += Show;
        boundManager.EventResolved += ShowResolved;

        if (boundManager.PendingEvent != null)
            Show(boundManager.PendingEvent);
    }

    private void Show(GameEventRuntime runtime)
    {
        if (runtime == null)
            return;

        if (root != null)
            root.SetActive(true);

        if (titleText != null)
            titleText.text = runtime.Definition.Title;

        if (descriptionText != null)
            descriptionText.text = runtime.Definition.Description;

        ClearOptions();

        if (runtime.IsAwaitingChoice)
        {
            if (consequenceText != null)
                consequenceText.text = "Выберите последствие";

            if (closeButton != null)
                closeButton.gameObject.SetActive(false);

            BuildChoiceButtons(runtime);
            return;
        }

        SetConsequenceText(runtime.SelectedOption);

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }

    private void ShowResolved(GameEventRuntime runtime)
    {
        if (runtime != null && !runtime.IsAwaitingChoice)
            Show(runtime);
    }

    private void BuildChoiceButtons(GameEventRuntime runtime)
    {
        IReadOnlyList<GameEventOption> options = runtime.Definition.GetCurrentOptions();

        for (int i = 0; i < options.Count; i++)
        {
            int optionIndex = i;
            Button button = CreateOptionButton();
            if (button == null)
                continue;

            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = options[i].Title;

            button.onClick.AddListener(() =>
            {
                if (boundManager != null)
                    boundManager.ResolvePendingChoice(optionIndex);
            });

            spawnedOptionButtons.Add(button);
        }
    }

    private Button CreateOptionButton()
    {
        if (optionsContainer == null)
            return null;

        if (optionButtonPrefab != null)
            return Instantiate(optionButtonPrefab, optionsContainer);

        GameObject buttonObject = new("EventOptionButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(optionsContainer, false);

        GameObject textObject = new("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;

        return buttonObject.GetComponent<Button>();
    }

    private void SetConsequenceText(GameEventOption option)
    {
        if (consequenceText == null)
            return;

        if (option == null)
        {
            consequenceText.text = string.Empty;
            return;
        }

        string effects = option.BuildEffectSummary();
        consequenceText.text = string.IsNullOrWhiteSpace(effects)
            ? option.Description
            : $"{option.Title}: {effects}";
    }

    private void Hide()
    {
        ClearOptions();

        if (root != null)
            root.SetActive(false);
    }

    private void ClearOptions()
    {
        for (int i = 0; i < spawnedOptionButtons.Count; i++)
        {
            if (spawnedOptionButtons[i] != null)
                Destroy(spawnedOptionButtons[i].gameObject);
        }

        spawnedOptionButtons.Clear();
    }
}
