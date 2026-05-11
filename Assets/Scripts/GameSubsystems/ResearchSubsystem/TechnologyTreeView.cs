using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyTreeView : MonoBehaviour
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TechnologyNodeView nodePrefab;
    [SerializeField] private Vector2 contentPadding = new Vector2(300f, 200f);
    [SerializeField] private RectTransform connectionsRoot;
    [SerializeField] private Image connectionPrefab;
    private List<Image> connectionLines = new();

    private readonly Dictionary<TechID, TechnologyNodeView> nodeViews = new();
    private bool isBuilt;

    private void Start()
    {
        BuildTree();
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

            foreach (TechID prereqId in childTech.Data.prerequisites)
            {
                if (!nodeViews.TryGetValue(prereqId, out TechnologyNodeView parentNode))
                    continue;

                if (!nodeViews.TryGetValue(childTech.Data.ID, out TechnologyNodeView childNode))
                    continue;

                CreateConnection(parentNode.GetComponent<RectTransform>(), childNode.GetComponent<RectTransform>());
            }
        }
    }

    private void CreateConnection(RectTransform from, RectTransform to)
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

        float branchOffset = 18f;

        if (endY > startY)
            startY += branchOffset;
        else if (endY < startY)
            startY -= branchOffset;

        if (Mathf.Abs(endY - fromPos.y) < 5f)
        {
            CreateHorizontalSegment(new Vector2(startX, fromPos.y), new Vector2(endX, fromPos.y));
            return;
        }

        float midX = (startX + endX) * 0.5f;

        CreateHorizontalSegment(new Vector2(startX, startY), new Vector2(midX, startY));
        CreateVerticalSegment(new Vector2(midX, startY), new Vector2(midX, endY));
        CreateHorizontalSegment(new Vector2(midX, endY), new Vector2(endX, endY));
    }

    private void CreateHorizontalSegment(Vector2 start, Vector2 end)
    {
        Image line = Instantiate(connectionPrefab, connectionsRoot);
        connectionLines.Add(line);

        RectTransform rect = line.rectTransform;

        float width = Mathf.Abs(end.x - start.x);
        float x = Mathf.Min(start.x, end.x) + width * 0.5f;
        float y = start.y;

        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, 8f);
        rect.localRotation = Quaternion.identity;
    }

    private void CreateVerticalSegment(Vector2 start, Vector2 end)
    {
        Image line = Instantiate(connectionPrefab, connectionsRoot);
        connectionLines.Add(line);

        RectTransform rect = line.rectTransform;

        float height = Mathf.Abs(end.y - start.y);
        float x = start.x;
        float y = Mathf.Min(start.y, end.y) + height * 0.5f;

        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(8f, height);
        rect.localRotation = Quaternion.identity;
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
}