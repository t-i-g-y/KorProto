using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyNodeView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image techImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image progressFill;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject lockedPrereqMarker;
    [SerializeField] private GameObject unlockedCheckmark;
    [SerializeField] private GameObject researchingHighlight;
    [SerializeField] private GameObject availableGlow;
    [SerializeField] private GameObject researchingPulse;
    [SerializeField] private CanvasGroup nodeCanvasGroup;

    private bool isCompactMode;
    private Technology boundTechnology;
    public Technology BoundTechnology => boundTechnology;

    public void Bind(Technology technology)
    {
        boundTechnology = technology;
        Refresh();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    private void OnEnable()
    {
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnResearchStateChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (ResearchSystem.Instance != null)
        {
            ResearchSystem.Instance.OnResearchStateChanged -= Refresh;
        }
    }

    private void OnClicked()
    {
        if (boundTechnology == null)
            return;

        ResearchSystem.Instance.StartResearch(boundTechnology.Data.ID);
    }

    public void Refresh()
    {
        if (boundTechnology == null)
            return;
        
        titleText.text = boundTechnology.Data.techName;
        costText.text = $"{boundTechnology.Progress}/{boundTechnology.Data.researchCost}";
        progressFill.fillAmount = boundTechnology.ProgressNormalized;

        if (techImage != null)
        {
            techImage.sprite = boundTechnology.Data.techImage;
            techImage.enabled = boundTechnology.Data.techImage != null;
        }
        
        bool canResearch = ResearchSystem.Instance.CanResearch(boundTechnology.Data.ID);

        bool isUnlocked = boundTechnology.IsUnlocked;
        bool isResearching = boundTechnology.IsResearching;
        bool isLocked = !canResearch && !isUnlocked;

        if (selectButton != null)
            selectButton.interactable = canResearch && !isUnlocked;

        if (lockedPrereqMarker != null)
            lockedPrereqMarker.SetActive(isLocked);

        if (unlockedCheckmark != null)
            unlockedCheckmark.SetActive(isUnlocked);

        if (researchingHighlight != null)
            researchingHighlight.SetActive(isResearching);

        if (availableGlow != null)
            availableGlow.SetActive(canResearch && !isUnlocked && !isResearching);

        if (researchingPulse != null)
            researchingPulse.SetActive(isResearching);

        if (nodeCanvasGroup != null)
            nodeCanvasGroup.alpha = isLocked ? 0.55f : 1f;
    }

    public void SetCompactMode(bool compactMode)
    {
        isCompactMode = compactMode;
        Refresh();
    }
}