using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestConditionType
{
    Manual,
    RailLineCount,
    TrainCount,
    StationsFoundByName,
    StationsConnectedByName,
    ResourceDeliveredBetweenStations,
    ArtifactCount
}

public enum QuestRewardType
{
    None,
    AddBalance,
    AddResearchPoints,
    AddArtifact
}

public enum QuestStatus
{
    Active = 0,
    Completed = 1,
    Inactive = 2
}

public class QuestWorldContext
{
    public int Day;
    public int Hour;
    public int RailLineCount;
    public int TrainCount;
    public int ArtifactCount;
    public ResourceType ResourceType;
    public int ResourceAmount;
    public string StartStationName;
    public string EndStationName;
}

[Serializable]
public class QuestCondition
{
    [SerializeField] private QuestConditionType conditionType = QuestConditionType.Manual;
    [SerializeField] private int count = 1;
    [SerializeField] private ResourceType resourceType = ResourceType.Coal;
    [SerializeField] private string stationName;
    [SerializeField] private string startStationName;
    [SerializeField] private string endStationName;

    public QuestConditionType ConditionType => conditionType;
    public int TargetCount => Mathf.Max(1, count);
    public bool IsResourceDelivery => conditionType == QuestConditionType.ResourceDeliveredBetweenStations;

    public bool Matches(QuestWorldContext context)
    {
        return GetProgress(context) >= TargetCount;
    }

    public int GetProgress(QuestWorldContext context)
    {
        if (context == null)
            return 0;

        return conditionType switch
        {
            QuestConditionType.Manual => 1,
            QuestConditionType.RailLineCount => context.RailLineCount,
            QuestConditionType.TrainCount => context.TrainCount,
            QuestConditionType.StationsFoundByName => CountFoundStations(),
            QuestConditionType.StationsConnectedByName => AreStationsConnected() ? 1 : 0,
            QuestConditionType.ResourceDeliveredBetweenStations => MatchesDeliveredResource(context) ? context.ResourceAmount : 0,
            QuestConditionType.ArtifactCount => context.ArtifactCount,
            _ => 0
        };
    }

    public int GetProgressDelta(QuestWorldContext context)
    {
        return conditionType == QuestConditionType.ResourceDeliveredBetweenStations && MatchesDeliveredResource(context)
            ? Mathf.Max(0, context.ResourceAmount)
            : 0;
    }

    public string BuildSummary()
    {
        return conditionType switch
        {
            QuestConditionType.Manual => "Выдается вручную",
            QuestConditionType.RailLineCount => $"Построить путей: {TargetCount}",
            QuestConditionType.TrainCount => $"Достичь количества поездов: {TargetCount}",
            QuestConditionType.StationsFoundByName => $"Найти станции: {BuildStationPairText()}",
            QuestConditionType.StationsConnectedByName => $"Соединить станции: {BuildStationPairText()}",
            QuestConditionType.ResourceDeliveredBetweenStations => $"Провезти {TargetCount} {resourceType} между станциями {BuildStationPairText()}",
            QuestConditionType.ArtifactCount => $"Найти артефактов: {TargetCount}",
            _ => "Не задано"
        };
    }

    private bool MatchesDeliveredResource(QuestWorldContext context)
    {
        if (context.ResourceType != resourceType)
            return false;

        return MatchesStationName(context.StartStationName, startStationName)
            && MatchesStationName(context.EndStationName, endStationName);
    }

    private int CountFoundStations()
    {
        int found = 0;

        if (!string.IsNullOrWhiteSpace(stationName) && FindStationByName(stationName) != null)
            found++;

        if (!string.IsNullOrWhiteSpace(startStationName) && FindStationByName(startStationName) != null)
            found++;

        if (!string.IsNullOrWhiteSpace(endStationName) && FindStationByName(endStationName) != null)
            found++;

        return found;
    }

    private bool AreStationsConnected()
    {
        Station start = FindStationByName(startStationName);
        Station end = FindStationByName(endStationName);

        return start != null
            && end != null
            && RailManager.Instance != null
            && RailManager.Instance.TryGetShortestPathFirstHop(start.Cell, end.Cell, out _, out _);
    }

    private string BuildStationPairText()
    {
        if (!string.IsNullOrWhiteSpace(stationName))
            return stationName;

        return $"{startStationName} - {endStationName}";
    }

    private static Station FindStationByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Station[] stations = UnityEngine.Object.FindObjectsByType<Station>(FindObjectsSortMode.None);
        foreach (Station station in stations)
        {
            if (station != null && MatchesStationName(station.StationName, name))
                return station;
        }

        return null;
    }

    private static bool MatchesStationName(string actual, string expected)
    {
        return !string.IsNullOrWhiteSpace(actual)
            && !string.IsNullOrWhiteSpace(expected)
            && string.Equals(actual.Trim(), expected.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}

[Serializable]
public class QuestReward
{
    [SerializeField] private QuestRewardType rewardType;
    [SerializeField] private float floatAmount;
    [SerializeField] private int intAmount;
    [SerializeField] private ArtifactDefinition artifactDefinition;

    public QuestReward()
    {
    }

    public QuestReward(QuestRewardType rewardType, float floatAmount = 0f, int intAmount = 0, ArtifactDefinition artifactDefinition = null)
    {
        this.rewardType = rewardType;
        this.floatAmount = floatAmount;
        this.intAmount = intAmount;
        this.artifactDefinition = artifactDefinition;
    }

    public void Apply()
    {
        switch (rewardType)
        {
            case QuestRewardType.AddBalance:
                FinanceSystem.Instance?.AdjustBalance(Mathf.Abs(floatAmount));
                break;
            case QuestRewardType.AddResearchPoints:
                ResearchSystem.Instance?.AddResearchPoints(Mathf.Max(0, intAmount));
                break;
            case QuestRewardType.AddArtifact:
                ArtifactManager.Instance?.AddArtifactToInventory(artifactDefinition);
                break;
        }
    }

    public string BuildSummary()
    {
        return rewardType switch
        {
            QuestRewardType.AddBalance => $"+{Mathf.Abs(floatAmount):0} к бюджету",
            QuestRewardType.AddResearchPoints => $"+{Mathf.Max(0, intAmount)} очков исследования",
            QuestRewardType.AddArtifact => artifactDefinition != null ? $"Артефакт: {artifactDefinition.Title}" : "Артефакт",
            _ => string.Empty
        };
    }
}

[CreateAssetMenu(menuName = "GameContent/Quests/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Header("Quest")]
    [SerializeField] private string questId;
    [SerializeField] private string title = "Задание";
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private bool canRepeat;

    [Header("Activation")]
    [SerializeField] private QuestCondition activationCondition = new();

    [Header("Objective")]
    [SerializeField] private QuestCondition objective = new();

    [Header("Rewards")]
    [SerializeField] private List<QuestReward> rewards = new();

    public string QuestId => string.IsNullOrWhiteSpace(questId) ? name : questId;
    public string Title => title;
    public string Description => description;
    public bool CanRepeat => canRepeat;
    public QuestCondition Objective => objective;
    public int TargetValue => objective.TargetCount;

    public bool Matches(QuestWorldContext context)
    {
        return activationCondition != null && activationCondition.Matches(context);
    }

    public int GetProgressFromContext(QuestWorldContext context)
    {
        return objective != null ? objective.GetProgress(context) : 0;
    }

    public string BuildActivationCondition()
    {
        return activationCondition != null ? activationCondition.BuildSummary() : "Условие появления не задано";
    }

    public string BuildObjectiveSummary()
    {
        return objective != null ? objective.BuildSummary() : "Цель не задана";
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
}
