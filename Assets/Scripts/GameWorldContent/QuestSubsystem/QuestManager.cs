using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private List<QuestDefinition> questDefinitions = new();

    private readonly List<QuestRuntime> activeQuests = new();
    private readonly List<QuestRuntime> completedQuests = new();
    private readonly HashSet<string> activatedQuestIds = new();
    private TimeManager boundTimeManager;
    private ArtifactManager boundArtifactManager;

    public static QuestManager Instance { get; private set; }
    public IReadOnlyList<QuestRuntime> ActiveQuests => activeQuests;
    public IReadOnlyList<QuestRuntime> CompletedQuests => completedQuests;

    public event Action<QuestRuntime> QuestActivated;
    public event Action<QuestRuntime> QuestProgressChanged;
    public event Action<QuestRuntime> QuestCompleted;
    public event Action QuestListChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null || FindAnyObjectByType<QuestManager>() != null)
            return;

        GameObject managerObject = new("QuestManager");
        managerObject.AddComponent<QuestManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        RailManager.LineCreated += HandleLineCreated;
        RailManager.LineRemoved += HandleLineRemoved;
        TrainManager.TrainCreated += HandleTrainCreated;
        TrainManager.TrainRemoved += HandleTrainRemoved;
        TryBindTimeManager();
        TryBindArtifactManager();
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        RailManager.LineRemoved -= HandleLineRemoved;
        TrainManager.TrainCreated -= HandleTrainCreated;
        TrainManager.TrainRemoved -= HandleTrainRemoved;
        UnbindTimeManager();
        UnbindArtifactManager();
    }

    private void Update()
    {
        if (boundTimeManager == null)
            TryBindTimeManager();

        if (boundArtifactManager == null)
            TryBindArtifactManager();
    }

    public IReadOnlyList<QuestRuntime> BuildJournalQuests()
    {
        List<QuestRuntime> quests = new();
        quests.AddRange(activeQuests);

        foreach (QuestDefinition definition in GetDefinitions())
        {
            if (definition == null || IsKnownQuest(definition.QuestId))
                continue;

            quests.Add(QuestRuntime.FromDefinition(definition, BuildContext(), QuestStatus.Inactive));
        }

        for (int i = completedQuests.Count - 1; i >= 0; i--)
            quests.Add(completedQuests[i]);

        return quests;
    }

    public void TriggerManualQuest(QuestDefinition definition)
    {
        if (definition == null)
            return;

        TryActivate(definition, BuildContext());
    }

    public void NotifyResourceDelivered(ResourceType resource, int amount, Station startStation, Station endStation)
    {
        QuestWorldContext context = BuildContext();
        context.ResourceType = resource;
        context.ResourceAmount = Mathf.Max(0, amount);
        context.StartStationName = startStation != null ? startStation.StationName : null;
        context.EndStationName = endStation != null ? endStation.StationName : null;

        EvaluateDefinitions(context);
        RefreshAllProgress(context);
    }

    public QuestManagerSaveData GetSaveData()
    {
        QuestManagerSaveData data = new();
        data.activeQuests.AddRange(activeQuests);
        data.completedQuests.AddRange(completedQuests);

        foreach (string questId in activatedQuestIds)
            data.activatedQuestIds.Add(questId);

        return data;
    }

    public void LoadFromSaveData(QuestManagerSaveData data)
    {
        activeQuests.Clear();
        completedQuests.Clear();
        activatedQuestIds.Clear();

        if (data != null)
        {
            activeQuests.AddRange(data.activeQuests);
            completedQuests.AddRange(data.completedQuests);

            foreach (string questId in data.activatedQuestIds)
            {
                if (!string.IsNullOrWhiteSpace(questId))
                    activatedQuestIds.Add(questId);
            }
        }

        RefreshAllProgress();
        QuestListChanged?.Invoke();
    }

    private IEnumerable<QuestDefinition> GetDefinitions()
    {
        foreach (QuestDefinition definition in questDefinitions)
        {
            if (definition != null)
                yield return definition;
        }
    }

    private void HandleLineCreated(RailLine line)
    {
        QuestWorldContext context = BuildContext();
        EvaluateDefinitions(context);
        RefreshAllProgress(context);
    }

    private void HandleLineRemoved(RailLine line)
    {
        RefreshAllProgress();
    }

    private void HandleTrainCreated(Train train, RailLine line)
    {
        QuestWorldContext context = BuildContext();
        EvaluateDefinitions(context);
        RefreshAllProgress(context);
    }

    private void HandleTrainRemoved(Train train)
    {
        RefreshAllProgress();
    }

    private void HandleArtifactCollected(ArtifactInventoryEntry entry)
    {
        QuestWorldContext context = BuildContext();
        EvaluateDefinitions(context);
        RefreshAllProgress(context);
    }

    private void HandleDayChanged(int day)
    {
        QuestWorldContext context = BuildContext();
        EvaluateDefinitions(context);
        RefreshAllProgress(context);
    }

    private void EvaluateDefinitions(QuestWorldContext context)
    {
        foreach (QuestDefinition definition in GetDefinitions())
            TryActivate(definition, context);
    }

    private bool TryActivate(QuestDefinition definition, QuestWorldContext context)
    {
        string questId = definition.QuestId;

        if (!definition.CanRepeat && activatedQuestIds.Contains(questId))
            return false;

        if (!definition.Matches(context))
            return false;

        QuestRuntime quest = QuestRuntime.FromDefinition(definition, context);
        activeQuests.Add(quest);
        activatedQuestIds.Add(questId);

        QuestActivated?.Invoke(quest);
        QuestListChanged?.Invoke();

        if (quest.IsComplete)
            CompleteQuest(quest, definition);

        return true;
    }

    private void RefreshAllProgress(QuestWorldContext context = null)
    {
        context ??= BuildContext();

        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            QuestRuntime quest = activeQuests[i];
            if (quest == null)
                continue;

            QuestDefinition definition = FindDefinition(quest.questId);
            if (definition == null)
                continue;

            int newProgress = definition.Objective != null && definition.Objective.IsResourceDelivery
                ? quest.progress + definition.Objective.GetProgressDelta(context)
                : definition.GetProgressFromContext(context);
            newProgress = Mathf.Clamp(newProgress, 0, quest.targetValue);
            if (quest.progress != newProgress)
            {
                quest.progress = newProgress;
                QuestProgressChanged?.Invoke(quest);
            }

            if (quest.IsComplete)
                CompleteQuest(quest, definition);
        }

        QuestListChanged?.Invoke();
    }

    private void CompleteQuest(QuestRuntime quest, QuestDefinition definition)
    {
        if (quest == null || !activeQuests.Remove(quest))
            return;

        quest.status = (int)QuestStatus.Completed;
        quest.progress = Mathf.Max(quest.progress, quest.targetValue);
        quest.completedDay = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0;
        quest.completedHour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0;

        definition?.ApplyRewards();
        completedQuests.Add(quest);

        QuestCompleted?.Invoke(quest);
        QuestListChanged?.Invoke();
    }

    private QuestDefinition FindDefinition(string questId)
    {
        foreach (QuestDefinition definition in GetDefinitions())
        {
            if (definition != null && definition.QuestId == questId)
                return definition;
        }

        return null;
    }

    private bool IsKnownQuest(string questId)
    {
        return activatedQuestIds.Contains(questId)
            || activeQuests.Exists(q => q != null && q.questId == questId)
            || completedQuests.Exists(q => q != null && q.questId == questId);
    }

    private QuestWorldContext BuildContext()
    {
        return new QuestWorldContext
        {
            Day = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0,
            Hour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0,
            RailLineCount = RailManager.Instance != null ? RailManager.Instance.Lines.Count : 0,
            TrainCount = TrainManager.Instance != null ? TrainManager.Instance.Trains.Count : 0,
            ArtifactCount = ArtifactManager.Instance != null ? ArtifactManager.Instance.Inventory.Count : 0,
        };
    }

    private void TryBindTimeManager()
    {
        if (boundTimeManager != null || TimeManager.Instance == null)
            return;

        boundTimeManager = TimeManager.Instance;
        boundTimeManager.OnDayChanged += HandleDayChanged;
    }

    private void UnbindTimeManager()
    {
        if (boundTimeManager == null)
            return;

        boundTimeManager.OnDayChanged -= HandleDayChanged;
        boundTimeManager = null;
    }

    private void TryBindArtifactManager()
    {
        if (boundArtifactManager != null || ArtifactManager.Instance == null)
            return;

        boundArtifactManager = ArtifactManager.Instance;
        boundArtifactManager.ArtifactCollected += HandleArtifactCollected;
    }

    private void UnbindArtifactManager()
    {
        if (boundArtifactManager == null)
            return;

        boundArtifactManager.ArtifactCollected -= HandleArtifactCollected;
        boundArtifactManager = null;
    }
}
