using System;
using UnityEngine;

public enum ArtifactTriggerType
{
    Manual,
    DayReached,
    DayInterval,
    RailLineCreated,
    RailLineCountReached,
    TrainCreated,
    TrainCountReached,
    TrainBroken,
    BalanceBelow,
    EventResolved,
    QuestCompleted
}

public enum ArtifactSpawnMode
{
    ContextRailLineEnd,
    ContextTrainCell,
    RandomStation,
    RandomRailLineCell,
    FixedCell,
    WorldOrigin
}

public class ArtifactWorldContext
{
    public ArtifactTriggerType TriggerType;
    public int Day;
    public int Hour;
    public int RailLineCount;
    public int TrainCount;
    public float Balance;
    public RailLine RailLine;
    public Train Train;
    public GameEventRuntime EventRuntime;
    public QuestRuntime Quest;
}

[CreateAssetMenu(menuName = "Rail/Artifacts/Artifact Definition")]
public class ArtifactDefinition : ScriptableObject
{
    [Header("Artifact")]
    [SerializeField] private string artifactId;
    [SerializeField] private string title = "Артефакт";
    [TextArea]
    [SerializeField] private string story;
    [SerializeField] private Sprite icon;

    [Header("Trigger")]
    [SerializeField] private ArtifactTriggerType triggerType = ArtifactTriggerType.Manual;
    [SerializeField] private float triggerValue;
    [SerializeField] private int minDay;
    [SerializeField] private string sourceIdFilter;
    [SerializeField] private bool canRepeat;

    [Header("Spawn")]
    [SerializeField] private ArtifactSpawnMode spawnMode = ArtifactSpawnMode.RandomRailLineCell;
    [SerializeField] private Vector3Int fixedCell;

    public string ArtifactId => string.IsNullOrWhiteSpace(artifactId) ? name : artifactId;
    public string Title => title;
    public string Story => story;
    public Sprite Icon => icon;
    public ArtifactTriggerType TriggerType => triggerType;
    public float TriggerValue => triggerValue;
    public int MinDay => minDay;
    public string SourceIdFilter => sourceIdFilter;
    public bool CanRepeat => canRepeat;
    public ArtifactSpawnMode SpawnMode => spawnMode;
    public Vector3Int FixedCell => fixedCell;

    public bool Matches(ArtifactWorldContext context)
    {
        if (context == null || context.TriggerType != triggerType)
            return false;

        if (context.Day < minDay)
            return false;

        int threshold = Mathf.RoundToInt(triggerValue);

        return triggerType switch
        {
            ArtifactTriggerType.Manual => true,
            ArtifactTriggerType.DayReached => context.Day >= threshold,
            ArtifactTriggerType.DayInterval => threshold > 0 && context.Day > 0 && context.Day % threshold == 0,
            ArtifactTriggerType.RailLineCreated => true,
            ArtifactTriggerType.RailLineCountReached => context.RailLineCount >= threshold,
            ArtifactTriggerType.TrainCreated => context.Train != null,
            ArtifactTriggerType.TrainCountReached => context.TrainCount >= threshold,
            ArtifactTriggerType.TrainBroken => context.Train != null,
            ArtifactTriggerType.BalanceBelow => context.Balance <= triggerValue,
            ArtifactTriggerType.EventResolved => MatchesSource(context.EventRuntime?.Definition?.EventId),
            ArtifactTriggerType.QuestCompleted => MatchesSource(context.Quest?.questId),
            _ => false
        };
    }

    private bool MatchesSource(string sourceId)
    {
        return string.IsNullOrWhiteSpace(sourceIdFilter) || sourceIdFilter == sourceId;
    }

    public static ArtifactDefinition CreateRuntime(
        string id,
        string artifactTitle,
        string artifactStory,
        ArtifactTriggerType artifactTriggerType,
        float artifactTriggerValue,
        ArtifactSpawnMode artifactSpawnMode,
        bool repeat,
        string sourceFilter = null)
    {
        ArtifactDefinition definition = CreateInstance<ArtifactDefinition>();
        definition.artifactId = id;
        definition.title = artifactTitle;
        definition.story = artifactStory;
        definition.triggerType = artifactTriggerType;
        definition.triggerValue = artifactTriggerValue;
        definition.spawnMode = artifactSpawnMode;
        definition.canRepeat = repeat;
        definition.sourceIdFilter = sourceFilter;

        return definition;
    }
}

