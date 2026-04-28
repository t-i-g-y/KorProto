using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactInventoryUI : MonoBehaviour
{
    [SerializeField] private ArtifactManager artifactManager;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform gridRoot;
    [SerializeField] private GameObject detailRoot;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailStoryText;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private int visibleSlotCount = 24;
    [SerializeField] private int gridColumns = 6;

    private readonly List<GameObject> spawnedSlots = new();
    private ArtifactManager boundManager;
    private ArtifactInventoryEntry selectedEntry;
    private GridLayoutGroup gridLayout;

    private void Awake()
    {
        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        SetDetail(null);
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
            boundManager.ArtifactCollected -= HandleArtifactCollected;
            boundManager.InventoryChanged -= Rebuild;
            boundManager = null;
        }
    }

    private void Update()
    {
        if (artifactManager == null)
            TryBindManager();
    }

    public void Configure(
        Button inventoryToggleButton,
        Button inventoryCloseButton,
        GameObject inventoryPanel,
        Transform inventoryGrid,
        GameObject detailsPanel,
        Image detailsIcon,
        TMP_Text detailsTitle,
        TMP_Text detailsStory,
        TMP_Text inventoryEmptyText)
    {
        toggleButton = inventoryToggleButton;
        closeButton = inventoryCloseButton;
        panelRoot = inventoryPanel;
        gridRoot = inventoryGrid;
        detailRoot = detailsPanel;
        detailIcon = detailsIcon;
        detailTitleText = detailsTitle;
        detailStoryText = detailsStory;
        emptyText = inventoryEmptyText;
        gridLayout = gridRoot != null ? gridRoot.GetComponent<GridLayoutGroup>() : null;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        ClosePanel();
        SetDetail(null);
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);

        if (panelRoot.activeSelf)
            Rebuild();
    }

    public void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Rebuild()
    {
        ClearSlots();

        IReadOnlyList<ArtifactInventoryEntry> inventory = boundManager != null
            ? boundManager.Inventory
            : null;

        int itemCount = inventory != null ? inventory.Count : 0;
        int slotCount = Mathf.Max(visibleSlotCount, itemCount);

        if (emptyText != null)
            emptyText.gameObject.SetActive(itemCount == 0);

        if (gridRoot == null)
            return;

        UpdateGridMetrics(slotCount);

        for (int i = 0; i < slotCount; i++)
        {
            ArtifactInventoryEntry entry = i < itemCount ? inventory[i] : null;
            SpawnSlot(entry);
        }

        if (selectedEntry != null && !ContainsEntry(inventory, selectedEntry.artifactId))
            SetDetail(null);
    }

    private void TryBindManager()
    {
        ArtifactManager manager = artifactManager != null ? artifactManager : ArtifactManager.Instance;
        if (manager == null)
            return;

        if (boundManager == manager)
            return;

        if (boundManager != null)
        {
            boundManager.ArtifactCollected -= HandleArtifactCollected;
            boundManager.InventoryChanged -= Rebuild;
        }

        boundManager = manager;
        boundManager.ArtifactCollected += HandleArtifactCollected;
        boundManager.InventoryChanged += Rebuild;
        Rebuild();
    }

    private void HandleArtifactCollected(ArtifactInventoryEntry entry)
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        Rebuild();
        SetDetail(entry);
    }

    private void SpawnSlot(ArtifactInventoryEntry entry)
    {
        GameObject slotObject = new("ArtifactSlot", typeof(RectTransform), typeof(Image), typeof(Button));
        slotObject.transform.SetParent(gridRoot, false);

        Image background = slotObject.GetComponent<Image>();
        background.color = entry == null
            ? new Color32(222, 224, 219, 255)
            : new Color32(246, 247, 241, 255);

        Button button = slotObject.GetComponent<Button>();
        button.interactable = entry != null;

        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image));
        iconObject.transform.SetParent(slotObject.transform, false);

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8f, 8f);
        iconRect.offsetMax = new Vector2(-8f, -8f);

        Image icon = iconObject.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.enabled = entry != null;

        if (entry != null)
        {
            ArtifactInventoryEntry capturedEntry = entry;
            icon.sprite = boundManager != null ? boundManager.GetIcon(entry.artifactId) : null;
            button.onClick.AddListener(() => SetDetail(capturedEntry));
        }

        spawnedSlots.Add(slotObject);
    }

    private void SetDetail(ArtifactInventoryEntry entry)
    {
        selectedEntry = entry;

        if (detailRoot != null)
            detailRoot.SetActive(entry != null);

        if (entry == null)
            return;

        if (detailIcon != null)
        {
            detailIcon.sprite = boundManager != null ? boundManager.GetIcon(entry.artifactId) : null;
            detailIcon.enabled = detailIcon.sprite != null;
        }

        if (detailTitleText != null)
            detailTitleText.text = entry.title;

        if (detailStoryText != null)
        {
            string time = entry.day > 0 ? $"День {entry.day}, {entry.hour:00}:00\n" : string.Empty;
            detailStoryText.text = $"{time}{entry.story}";
        }
    }

    private void UpdateGridMetrics(int slotCount)
    {
        if (gridLayout == null)
            gridLayout = gridRoot.GetComponent<GridLayoutGroup>();

        if (gridLayout == null)
            return;

        RectTransform gridRect = gridRoot as RectTransform;
        RectTransform viewportRect = gridRoot.parent as RectTransform;
        RectTransform sourceRect = viewportRect != null ? viewportRect : gridRect;
        float availableWidth = sourceRect != null && sourceRect.rect.width > 0f
            ? sourceRect.rect.width
            : 720f;

        int columns = Mathf.Clamp(gridColumns, 1, Mathf.Max(1, slotCount));
        float spacing = 14f;
        float horizontalPadding = 28f;
        float cellSize = Mathf.Floor((availableWidth - horizontalPadding - spacing * (columns - 1)) / columns);
        cellSize = Mathf.Clamp(cellSize, 64f, 112f);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.spacing = new Vector2(spacing, spacing);
        gridLayout.padding = new RectOffset(14, 14, 14, 14);
        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }

    private static bool ContainsEntry(IReadOnlyList<ArtifactInventoryEntry> inventory, string artifactId)
    {
        if (inventory == null || string.IsNullOrWhiteSpace(artifactId))
            return false;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] != null && inventory[i].artifactId == artifactId)
                return true;
        }

        return false;
    }

    private void ClearSlots()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
                Destroy(spawnedSlots[i]);
        }

        spawnedSlots.Clear();
    }
}
