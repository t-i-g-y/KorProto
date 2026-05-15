using UnityEngine;
using UnityEngine.UI;

public class ResearchMenuController : MonoBehaviour
{
    [SerializeField] private Button researchToggleButton;
    [SerializeField] private Button viewTreeButton;
    [SerializeField] private Button closeTreeButton;
    [SerializeField] private GameObject compactResearchPanel;
    [SerializeField] private GameObject fullTreePanel;
    [SerializeField] private TechnologyNodeView currentResearchNodePrefab;
    [SerializeField] private GameObject noResearchPlaceholder;

    private void Awake()
    {
        if (researchToggleButton != null)
            researchToggleButton.onClick.AddListener(ToggleCompactPanel);

        if (viewTreeButton != null)
            viewTreeButton.onClick.AddListener(OpenTree);

        if (closeTreeButton != null)
            closeTreeButton.onClick.AddListener(CloseTree);

        SetCompactPanel(false);
        SetTreePanel(false);
    }

    private void OnEnable()
    {
        if (ResearchSystem.Instance != null)
            ResearchSystem.Instance.OnResearchStateChanged += RefreshCurrentResearchView;
        
        RefreshCurrentResearchView();
    }

    private void OnDisable()
    {
        if (ResearchSystem.Instance != null)
            ResearchSystem.Instance.OnResearchStateChanged -= RefreshCurrentResearchView;
    }

    public void ToggleCompactPanel()
    {
        bool nextState = !compactResearchPanel.activeSelf;

        SetCompactPanel(nextState);

        if (nextState)
            RefreshCurrentResearchView();
    }

    public void OpenTree()
    {
        SetCompactPanel(false);
        SetTreePanel(true);
    }

    public void CloseTree()
    {
        SetTreePanel(false);
        RefreshCurrentResearchView();
        SetCompactPanel(true);
    }

    private void SetCompactPanel(bool value)
    {
        if (compactResearchPanel != null)
            compactResearchPanel.SetActive(value);
    }

    private void SetTreePanel(bool value)
    {
        if (fullTreePanel != null)
            fullTreePanel.SetActive(value);
    }

    public void RefreshCurrentResearchView()
    {
        if (currentResearchNodePrefab == null || ResearchSystem.Instance == null)
            return;

        Technology current = ResearchSystem.Instance.CurrentResearch;

        if (current == null)
        {
            currentResearchNodePrefab.gameObject.SetActive(false);
            noResearchPlaceholder.gameObject.SetActive(true);
            return;
        }

        if (noResearchPlaceholder.gameObject.activeSelf)
        {
            noResearchPlaceholder.SetActive(false);
        }
        
        currentResearchNodePrefab.gameObject.SetActive(true);
        currentResearchNodePrefab.Bind(current);
        currentResearchNodePrefab.SetCompactMode(true);
    }
}
