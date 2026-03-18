using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyNodeView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image techImage;
    [SerializeField] private Image progressFill;
    [SerializeField] private Button selectButton;
    [SerializeField] private GameObject lockedPrereqMarker;
    [SerializeField] private GameObject unlockedCheckmark;
    [SerializeField] private GameObject researchingHighlight;
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

        selectButton.interactable = canResearch && !boundTechnology.IsUnlocked;
        lockedPrereqMarker.SetActive(!canResearch && !boundTechnology.IsUnlocked);
        unlockedCheckmark.SetActive(boundTechnology.IsUnlocked);
        researchingHighlight.SetActive(boundTechnology.IsResearching);
    }

    public void SetCompactMode(bool compactMode)
    {
        isCompactMode = compactMode;
        Refresh();
    }
}