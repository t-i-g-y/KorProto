using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestJournalEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;

    public void Set(QuestRuntime quest)
    {
        if (quest == null)
            return;

        if (titleText != null)
        {
            titleText.color = Color.black;
            titleText.text = quest.title;
        }

        if (statusText != null)
        {
            statusText.color = Color.black;
            statusText.text = quest.Status == QuestStatus.Completed ? "Выполнено" : "Активно";
        }

        if (descriptionText != null)
        {
            descriptionText.color = Color.black;
            descriptionText.text = quest.description;
        }

        if (conditionText != null)
        {
            conditionText.color = Color.black;
            conditionText.text = $"Условия: {quest.activationCondition}. Цель: {quest.objectiveSummary}";
        }

        if (rewardText != null)
        {
            rewardText.color = Color.black;
            rewardText.text = $"Награда: {quest.rewardSummary}";
        }

        if (progressText != null)
        {
            progressText.color = Color.black;
            progressText.text = $"Прогресс: {quest.progress}/{quest.targetValue}";
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = Mathf.Max(1, quest.targetValue);
            progressSlider.value = Mathf.Clamp(quest.progress, 0, quest.targetValue);
        }
    }
}
