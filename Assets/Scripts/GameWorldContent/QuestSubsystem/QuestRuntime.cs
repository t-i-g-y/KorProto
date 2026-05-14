using System;
using UnityEngine;

[Serializable]
public class QuestRuntime
{
    public string questId;
    public string title;
    public string description;
    public string activationCondition;
    public string objectiveSummary;
    public string rewardSummary;
    public int targetValue;
    public int progress;
    public int status;
    public int activatedDay;
    public int activatedHour;
    public int completedDay;
    public int completedHour;

    public QuestStatus Status => (QuestStatus)status;
    public float NormalizedProgress => targetValue <= 0 ? 1f : Mathf.Clamp01(progress / (float)targetValue);
    public bool IsComplete => progress >= targetValue;

    public static QuestRuntime FromDefinition(QuestDefinition definition, QuestWorldContext context, QuestStatus questStatus = QuestStatus.Active)
    {
        int progress = definition.Objective != null && definition.Objective.IsResourceDelivery
            ? 0
            : Mathf.Clamp(definition.GetProgressFromContext(context), 0, definition.TargetValue);

        return new QuestRuntime
        {
            questId = definition.QuestId,
            title = definition.Title,
            description = definition.Description,
            activationCondition = definition.BuildActivationCondition(),
            objectiveSummary = definition.BuildObjectiveSummary(),
            rewardSummary = definition.BuildRewardSummary(),
            targetValue = definition.TargetValue,
            progress = progress,
            status = (int)questStatus,
            activatedDay = context != null ? context.Day : 0,
            activatedHour = context != null ? context.Hour : 0
        };
    }
}
