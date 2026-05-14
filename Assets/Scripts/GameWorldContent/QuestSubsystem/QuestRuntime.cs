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
    public int objectiveType;
    public int targetValue;
    public int progress;
    public int status;
    public int activatedDay;
    public int activatedHour;
    public int completedDay;
    public int completedHour;

    public QuestStatus Status => (QuestStatus)status;
    public QuestObjectiveType ObjectiveType => (QuestObjectiveType)objectiveType;
    public float NormalizedProgress => targetValue <= 0 ? 1f : Mathf.Clamp01(progress / (float)targetValue);
    public bool IsComplete => progress >= targetValue;

    public static QuestRuntime FromDefinition(QuestDefinition definition, QuestWorldContext context)
    {
        int progress = Mathf.Clamp(definition.GetProgressFromContext(context), 0, definition.TargetValue);

        return new QuestRuntime
        {
            questId = definition.QuestId,
            title = definition.Title,
            description = definition.Description,
            activationCondition = definition.BuildActivationCondition(),
            objectiveSummary = definition.BuildObjectiveSummary(),
            rewardSummary = definition.BuildRewardSummary(),
            objectiveType = (int)definition.ObjectiveType,
            targetValue = definition.TargetValue,
            progress = progress,
            status = (int)QuestStatus.Active,
            activatedDay = context != null ? context.Day : 0,
            activatedHour = context != null ? context.Hour : 0
        };
    }
}
