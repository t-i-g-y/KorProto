using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestTriggerType
{
    Manual,
    DayReached,
    RailLineCreated,
    RailLineCountReached,
    TrainCountReached,
    BalanceBelow
}

public enum QuestObjectiveType
{
    BuildRailLines,
    BuildRailTiles,
    OwnTrains,
    ReachBalance
}

public enum QuestRewardType
{
    None,
    AdjustBalance,
    AddResearchPoints
}

public enum QuestStatus
{
    Active,
    Completed
}

public class QuestWorldContext
{
    public QuestTriggerType TriggerType;
    public int Day;
    public int Hour;
    public int RailLineCount;
    public int TotalRailTiles;
    public int TrainCount;
    public float Balance;
    public RailLine RailLine;
}

[Serializable]
public class QuestReward
{
    [SerializeField] private QuestRewardType rewardType;
    [SerializeField] private float floatAmount;
    [SerializeField] private int intAmount;

    public QuestReward()
    {
    }

    public QuestReward(QuestRewardType rewardType, float floatAmount = 0f, int intAmount = 0)
    {
        this.rewardType = rewardType;
        this.floatAmount = floatAmount;
        this.intAmount = intAmount;
    }

    public QuestRewardType RewardType => rewardType;
    public float FloatAmount => floatAmount;
    public int IntAmount => intAmount;

    public void Apply()
    {
        switch (rewardType)
        {
            case QuestRewardType.AdjustBalance:
                FinanceSystem.Instance?.AdjustBalance(floatAmount);
                break;
            case QuestRewardType.AddResearchPoints:
                ResearchSystem.Instance?.AddResearchPoints(Mathf.Max(0, intAmount));
                break;
        }
    }

    public string BuildSummary()
    {
        return rewardType switch
        {
            QuestRewardType.AdjustBalance => floatAmount >= 0f ? $"+{floatAmount:0} к бюджету" : $"{floatAmount:0} к бюджету",
            QuestRewardType.AddResearchPoints => $"+{Mathf.Max(0, intAmount)} очков исследования",
            _ => string.Empty
        };
    }
}

[CreateAssetMenu(menuName = "Rail/Quests/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Quest")]
    [SerializeField] private string questId;
    [SerializeField] private string title = "Задание";
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private bool canRepeat;

    [Header("Activation")]
    [SerializeField] private QuestTriggerType triggerType = QuestTriggerType.Manual;
    [SerializeField] private float triggerValue;
    [SerializeField] private int minDay;

    [Header("Objective")]
    [SerializeField] private QuestObjectiveType objectiveType = QuestObjectiveType.BuildRailLines;
    [SerializeField] private int targetValue = 1;

    [Header("Rewards")]
    [SerializeField] private List<QuestReward> rewards = new();

    public string QuestId => string.IsNullOrWhiteSpace(questId) ? name : questId;
    public string Title => title;
    public string Description => description;
    public bool CanRepeat => canRepeat;
    public QuestTriggerType TriggerType => triggerType;
    public float TriggerValue => triggerValue;
    public int MinDay => minDay;
    public QuestObjectiveType ObjectiveType => objectiveType;
    public int TargetValue => Mathf.Max(1, targetValue);
    public IReadOnlyList<QuestReward> Rewards => rewards;

    public bool Matches(QuestWorldContext context)
    {
        if (context == null || context.TriggerType != triggerType)
            return false;

        if (context.Day < minDay)
            return false;

        int threshold = Mathf.RoundToInt(triggerValue);

        return triggerType switch
        {
            QuestTriggerType.Manual => true,
            QuestTriggerType.DayReached => context.Day >= threshold,
            QuestTriggerType.RailLineCreated => true,
            QuestTriggerType.RailLineCountReached => context.RailLineCount >= threshold,
            QuestTriggerType.TrainCountReached => context.TrainCount >= threshold,
            QuestTriggerType.BalanceBelow => context.Balance <= triggerValue,
            _ => false
        };
    }

    public int GetProgressFromContext(QuestWorldContext context)
    {
        if (context == null)
            return 0;

        return objectiveType switch
        {
            QuestObjectiveType.BuildRailLines => context.RailLineCount,
            QuestObjectiveType.BuildRailTiles => context.TotalRailTiles,
            QuestObjectiveType.OwnTrains => context.TrainCount,
            QuestObjectiveType.ReachBalance => Mathf.FloorToInt(context.Balance),
            _ => 0
        };
    }

    public string BuildActivationCondition()
    {
        int threshold = Mathf.RoundToInt(triggerValue);

        return triggerType switch
        {
            QuestTriggerType.Manual => "Выдается вручную",
            QuestTriggerType.DayReached => $"Появляется с дня {threshold}",
            QuestTriggerType.RailLineCreated => "Появляется после строительства пути",
            QuestTriggerType.RailLineCountReached => $"Появляется после строительства {threshold} путей",
            QuestTriggerType.TrainCountReached => $"Появляется при наличии {threshold} поездов",
            QuestTriggerType.BalanceBelow => $"Появляется при бюджете ниже {triggerValue:0}",
            _ => "Условие появления не задано"
        };
    }

    public string BuildObjectiveSummary()
    {
        return objectiveType switch
        {
            QuestObjectiveType.BuildRailLines => $"Построить {TargetValue} путей",
            QuestObjectiveType.BuildRailTiles => $"Построить {TargetValue} клеток пути",
            QuestObjectiveType.OwnTrains => $"Иметь {TargetValue} поездов",
            QuestObjectiveType.ReachBalance => $"Достичь бюджета {TargetValue}",
            _ => "Цель не задана"
        };
    }

    public string BuildRewardSummary()
    {
        List<string> parts = new();

        foreach (QuestReward reward in rewards)
        {
            if (reward == null)
                continue;

            string summary = reward.BuildSummary();
            if (!string.IsNullOrWhiteSpace(summary))
                parts.Add(summary);
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "Без награды";
    }

    public void ApplyRewards()
    {
        foreach (QuestReward reward in rewards)
            reward?.Apply();
    }

    public static QuestDefinition CreateRuntime(
        string id,
        string questTitle,
        string questDescription,
        QuestTriggerType questTriggerType,
        float questTriggerValue,
        QuestObjectiveType questObjectiveType,
        int questTargetValue,
        bool repeat,
        params QuestReward[] questRewards)
    {
        QuestDefinition definition = CreateInstance<QuestDefinition>();
        definition.questId = id;
        definition.title = questTitle;
        definition.description = questDescription;
        definition.triggerType = questTriggerType;
        definition.triggerValue = questTriggerValue;
        definition.objectiveType = questObjectiveType;
        definition.targetValue = Mathf.Max(1, questTargetValue);
        definition.canRepeat = repeat;
        definition.rewards = questRewards != null ? new List<QuestReward>(questRewards) : new List<QuestReward>();

        return definition;
    }
}
