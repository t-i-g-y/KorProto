using System.Collections.Generic;
using UnityEngine;

public class TechnologyTreeView : MonoBehaviour
{
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TechnologyNodeView nodePrefab;

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

        foreach (KeyValuePair<TechID, Technology> pair in allTechs)
        {
            Technology tech = pair.Value;

            TechnologyNodeView node = Instantiate(nodePrefab, contentRoot);
            node.Bind(tech);
            node.SetCompactMode(false);

            RectTransform nodeRect = node.GetComponent<RectTransform>();
            nodeRect.anchoredPosition = tech.Data.technologyTreePos;

            nodeViews.Add(pair.Key, node);
        }
    }

    public void Refresh()
    {
        foreach (TechnologyNodeView node in nodeViews.Values)
        {
            if (node != null)
                node.Refresh();
        }
    }

    public TechnologyNodeView GetNodeView(TechID id)
    {
        nodeViews.TryGetValue(id, out TechnologyNodeView node);
        return node;
    }

    private void ClearTree()
    {
        foreach (Transform child in contentRoot)
        {
            Destroy(child.gameObject);
        }

        nodeViews.Clear();
    }

    public void OnEnable()
    {
        Refresh();
    }
}