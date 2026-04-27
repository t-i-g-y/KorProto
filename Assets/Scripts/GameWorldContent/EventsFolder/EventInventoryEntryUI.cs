using TMPro;
using UnityEngine;

public class EventInventoryEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text consequenceText;

    public void Set(EventHistoryEntry entry)
    {
        if (entry == null)
            return;

        if (titleText != null)
            titleText.text = entry.title;

        if (timeText != null)
            timeText.text = $"День {entry.day}, {entry.hour:00}:00";

        if (descriptionText != null)
            descriptionText.text = entry.description;

        if (consequenceText != null)
        {
            string consequence = string.IsNullOrWhiteSpace(entry.consequenceEffects)
                ? entry.consequenceDescription
                : entry.consequenceEffects;

            consequenceText.text = $"{entry.consequenceTitle}: {consequence}";
        }
    }
}
