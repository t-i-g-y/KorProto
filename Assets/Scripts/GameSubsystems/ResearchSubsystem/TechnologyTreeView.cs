using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TechnologyTreeView : MonoBehaviour
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TechnologyNodeView nodePrefab;
    [SerializeField] private Vector2 contentPadding = new Vector2(300f, 200f);
    [SerializeField] private RectTransform connectionsRoot;
    [SerializeField] private RectTransform mainConnectionRoot;
    [SerializeField] private RectTransform branchConnectionRoot;
    [SerializeField] private Image connectionPrefab;
    [SerializeField] private float mainConnectionThickness = 8f;
    [SerializeField] private float branchConnectionThickness = 4f;
    [SerializeField] private Color mainConnectionColor;
    [SerializeField] private Color branchConnectionColor;
    [SerializeField] private float branchOffset;
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private Vector2 tooltipOffset = new Vector2(24f, -24f);
    private List<Image> connectionLines = new();

    private readonly Dictionary<TechID, TechnologyNodeView> nodeViews = new();
    private bool isBuilt;

    private void Start()
    {
        BuildTree();
    }

    private void Update()
    {
        if (Mouse.current == null || tooltipRoot == null || !tooltipRoot.activeSelf || tooltipRect == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        tooltipRect.position = mousePosition + tooltipOffset;
    }
    public void BuildTree()
    {
        ClearTree();

        if (ResearchSystem.Instance == null)
        {
            Debug.LogError("TechnologyTreeView:ResearchSystem.Instance missing");
            return;
        }

        Dictionary<TechID, Technology> allTechs = ResearchSystem.Instance.GetAllTechnologies();

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var tech in allTechs)
        {
            Technology technology = tech.Value;

            TechnologyNodeView node = Instantiate(nodePrefab, contentRoot);
            node.Bind(technology);
            node.SetCompactMode(false);
            BindTooltip(node, technology);
            RectTransform nodeRect = node.GetComponent<RectTransform>();
            nodeRect.anchoredPosition = technology.Data.technologyTreePos;

            nodeViews.Add(tech.Key, node);

            Vector2 pos = technology.Data.technologyTreePos;
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }

        ResizeContent(minX, maxX, minY, maxY);
        BuildConnections(allTechs);
    }

    private void ResizeContent(float minX, float maxX, float minY, float maxY)
    {
        if (contentRoot == null || nodeViews.Count == 0)
            return;

        float width = maxX + contentPadding.x;
        float height = (maxY - minY) + contentPadding.y;

        width = Mathf.Max(width, 2000f);
        height = Mathf.Max(height, 800f);

        contentRoot.sizeDelta = new Vector2(width, height);
    }

    private void BuildConnections(Dictionary<TechID, Technology> allTechs)
    {
        ClearConnections();

        foreach (KeyValuePair<TechID, Technology> pair in allTechs)
        {
            Technology childTech = pair.Value;
            if (childTech == null || childTech.Data == null)
                continue;

            for (int i = 0; i < childTech.Data.prerequisites.Count; i++)
            {
                TechID prereqId = childTech.Data.prerequisites[i];

                if (!nodeViews.TryGetValue(prereqId, out TechnologyNodeView parentNode))
                    continue;

                if (!nodeViews.TryGetValue(childTech.Data.ID, out TechnologyNodeView childNode))
                    continue;

                bool isBranch = i > 0;

                CreateConnection(parentNode.GetComponent<RectTransform>(), childNode.GetComponent<RectTransform>(), isBranch);
            }
        }
    }

    private void CreateConnection(RectTransform from, RectTransform to, bool isBranch)
    {
        if (connectionPrefab == null || connectionsRoot == null || from== null || to == null)
            return;

        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;

        float fromHalfWidth = from.rect.width * 0.5f;
        float toHalfWidth = to.rect.width * 0.5f;

        float startX = fromPos.x + fromHalfWidth;
        float endX = toPos.x - toHalfWidth;

        float startY = fromPos.y;
        float endY = toPos.y;

        if (endY > startY)
        {
            startY += branchOffset;
            if (isBranch)
                endY -= branchOffset;
        }
        else if (endY < startY)
        {
            startY -= branchOffset;
            if (isBranch)
                endY += branchOffset;
        }

        float lineThickness = isBranch ? branchConnectionThickness : mainConnectionThickness;
        Color lineColor = isBranch ? branchConnectionColor : mainConnectionColor;
        if (Mathf.Abs(endY - fromPos.y) < 5f)
        {
            CreateHorizontalSegment(new Vector2(startX, fromPos.y), new Vector2(endX, fromPos.y), lineThickness, lineColor);
            return;
        }

        float midX = (startX + endX) * 0.5f;

        CreateHorizontalSegment(new Vector2(startX, startY), new Vector2(midX, startY), lineThickness, lineColor);
        CreateVerticalSegment(new Vector2(midX, startY), new Vector2(midX, endY), lineThickness, lineColor);
        CreateHorizontalSegment(new Vector2(midX, endY), new Vector2(endX, endY), lineThickness, lineColor);
    }

    private void CreateHorizontalSegment(Vector2 start, Vector2 end, float thickness, Color color)
    {
        RectTransform root = thickness == mainConnectionThickness ? mainConnectionRoot : branchConnectionRoot;
        Image line = Instantiate(connectionPrefab, root);
        connectionLines.Add(line);

        RectTransform rect = line.rectTransform;

        float width = Mathf.Abs(end.x - start.x);
        float x = Mathf.Min(start.x, end.x) + width * 0.5f;
        float y = start.y;

        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, thickness);
        rect.localRotation = Quaternion.identity;
        line.color = color;
    }

    private void CreateVerticalSegment(Vector2 start, Vector2 end, float thickness, Color color)
    {
        RectTransform root = thickness == mainConnectionThickness ? mainConnectionRoot : branchConnectionRoot;
        Image line = Instantiate(connectionPrefab, root);
        connectionLines.Add(line);

        RectTransform rect = line.rectTransform;

        float height = Mathf.Abs(end.y - start.y);
        float x = start.x;
        float y = Mathf.Min(start.y, end.y) + height * 0.5f;

        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(thickness, height);
        rect.localRotation = Quaternion.identity;
        line.color = color;
    }

    private void ClearConnections()
    {
        for (int i = 0; i < connectionLines.Count; i++)
        {
            if (connectionLines[i] != null)
                Destroy(connectionLines[i].gameObject);
        }

        connectionLines.Clear();
    }
    public void Refresh()
    {
        foreach (TechnologyNodeView node in nodeViews.Values)
            if (node != null)
                node.Refresh();
    }

    public TechnologyNodeView GetNodeView(TechID id)
    {
        nodeViews.TryGetValue(id, out TechnologyNodeView node);
        return node;
    }

    private void ClearTree()
    {
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);
        ClearConnections();
        nodeViews.Clear();
    }

    public void OnEnable()
    {
        Refresh();
    }

    private void BindTooltip(TechnologyNodeView node, Technology technology)
    {
        if (node == null || technology == null)
            return;

        EventTrigger trigger = node.GetComponent<EventTrigger>();
        if (trigger == null)
            return;

        trigger.triggers.Clear();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener(_ => ShowTooltip(technology));
        EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ => HideTooltip());

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    private void ShowTooltip(Technology technology)
    {
        if (technology == null || technology.Data == null)
            return;
        string description = "";
        if (tooltipText != null)
            description = technology.Data.techDescription;

        description = description.Replace("\\n", "\n").Replace("\t", "    ");

        tooltipText.text = description;
        if (tooltipRoot != null)
            tooltipRoot.SetActive(true);
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }
}