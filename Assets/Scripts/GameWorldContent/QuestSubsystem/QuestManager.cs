using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private List<QuestDefinition> questDefinitions = new();
    [SerializeField] private bool useBuiltInQuestsWhenEmpty = true;

    private readonly List<QuestDefinition> runtimeDefinitions = new();
    private readonly List<QuestRuntime> activeQuests = new();
    private readonly List<QuestRuntime> completedQuests = new();
    private readonly HashSet<string> activatedQuestIds = new();

    private TimeManager boundTimeManager;

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

        GameObject managerObject = new GameObject("QuestManager");
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
        BuildRuntimeDefinitions();
    }

    private void OnEnable()
    {
        RailManager.LineCreated += HandleLineCreated;
        RailManager.LineRemoved += HandleLineRemoved;
        TrainManager.TrainCreated += HandleTrainCreated;
        TryBindTimeManager();
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        RailManager.LineRemoved -= HandleLineRemoved;
        TrainManager.TrainCreated -= HandleTrainCreated;
        UnbindTimeManager();
    }

    private void Update()
    {
        if (boundTimeManager == null)
            TryBindTimeManager();
    }

    public void TriggerManualQuest(QuestDefinition definition)
    {
        if (definition == null)
            return;

        TryActivate(definition, BuildContext(QuestTriggerType.Manual));
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

    private void BuildRuntimeDefinitions()
    {
        runtimeDefinitions.Clear();

        if (!useBuiltInQuestsWhenEmpty)
            return;

        runtimeDefinitions.Add(QuestDefinition.CreateRuntime(
            "builtin_build_5_rail_lines",
            "Первые пять путей",
            "Расширьте железнодорожную сеть, чтобы закрепить первые маршруты.",
            QuestTriggerType.RailLineCountReached,
            1f,
            QuestObjectiveType.BuildRailLines,
            5,
            false,
            new QuestReward(QuestRewardType.AdjustBalance, 75f)));
    }

    private IEnumerable<QuestDefinition> GetActiveDefinitions()
    {
        bool hasConfiguredDefinitions = false;

        foreach (QuestDefinition definition in questDefinitions)
        {
            if (definition == null)
                continue;

            hasConfiguredDefinitions = true;
            yield return definition;
        }

        if (hasConfiguredDefinitions || !useBuiltInQuestsWhenEmpty)
            yield break;

        foreach (QuestDefinition definition in runtimeDefinitions)
            yield return definition;
    }

    private void HandleLineCreated(RailLine line)
    {
        QuestWorldContext context = BuildContext(QuestTriggerType.RailLineCreated);
        context.RailLine = line;
        EvaluateTrigger(QuestTriggerType.RailLineCreated, context);

        context.TriggerType = QuestTriggerType.RailLineCountReached;
        EvaluateTrigger(QuestTriggerType.RailLineCountReached, context);

        RefreshProgressForObjective(QuestObjectiveType.BuildRailLines, context);
        RefreshProgressForObjective(QuestObjectiveType.BuildRailTiles, context);
    }

    private void HandleLineRemoved(RailLine line)
    {
        QuestWorldContext context = BuildContext(QuestTriggerType.RailLineCountReached);
        RefreshProgressForObjective(QuestObjectiveType.BuildRailLines, context);
        RefreshProgressForObjective(QuestObjectiveType.BuildRailTiles, context);
    }

    private void HandleTrainCreated(Train train, RailLine line)
    {
        QuestWorldContext context = BuildContext(QuestTriggerType.TrainCountReached);
        EvaluateTrigger(QuestTriggerType.TrainCountReached, context);
        RefreshProgressForObjective(QuestObjectiveType.OwnTrains, context);
    }

    private void HandleDayChanged(int day)
    {
        QuestWorldContext context = BuildContext(QuestTriggerType.DayReached);
        EvaluateTrigger(QuestTriggerType.DayReached, context);
        RefreshProgressForObjective(QuestObjectiveType.ReachBalance, context);
    }

    private void EvaluateTrigger(QuestTriggerType triggerType, QuestWorldContext context)
    {
        context.TriggerType = triggerType;

        foreach (QuestDefinition definition in GetActiveDefinitions())
        {
            if (definition == null || definition.TriggerType != triggerType)
                continue;

            TryActivate(definition, context);
        }
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
        Debug.Log($"Задание получено: {quest.title}");

        if (quest.IsComplete)
            CompleteQuest(quest, definition);

        return true;
    }

    private void RefreshAllProgress()
    {
        QuestWorldContext context = BuildContext(QuestTriggerType.Manual);
        RefreshProgressForObjective(QuestObjectiveType.BuildRailLines, context);
        RefreshProgressForObjective(QuestObjectiveType.BuildRailTiles, context);
        RefreshProgressForObjective(QuestObjectiveType.OwnTrains, context);
        RefreshProgressForObjective(QuestObjectiveType.ReachBalance, context);
    }

    private void RefreshProgressForObjective(QuestObjectiveType objectiveType, QuestWorldContext context)
    {
        for (int i = activeQuests.Count - 1; i >= 0; i--)
        {
            QuestRuntime quest = activeQuests[i];
            if (quest == null || quest.ObjectiveType != objectiveType)
                continue;

            int newProgress = Mathf.Clamp(GetObjectiveProgress(objectiveType, context), 0, quest.targetValue);
            if (quest.progress != newProgress)
            {
                quest.progress = newProgress;
                QuestProgressChanged?.Invoke(quest);
            }

            if (quest.IsComplete)
                CompleteQuest(quest, FindDefinition(quest.questId));
        }

        QuestListChanged?.Invoke();
    }

    private int GetObjectiveProgress(QuestObjectiveType objectiveType, QuestWorldContext context)
    {
        if (context == null)
            context = BuildContext(QuestTriggerType.Manual);

        return objectiveType switch
        {
            QuestObjectiveType.BuildRailLines => context.RailLineCount,
            QuestObjectiveType.BuildRailTiles => context.TotalRailTiles,
            QuestObjectiveType.OwnTrains => context.TrainCount,
            QuestObjectiveType.ReachBalance => Mathf.FloorToInt(context.Balance),
            _ => 0
        };
    }

    private void CompleteQuest(QuestRuntime quest, QuestDefinition definition)
    {
        if (quest == null || !activeQuests.Remove(quest))
            return;

        quest.status = (int)QuestStatus.Completed;
        quest.progress = Mathf.Max(quest.progress, quest.targetValue);
        quest.completedDay = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0;
        quest.completedHour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0;

        if (definition != null)
            definition.ApplyRewards();

        completedQuests.Add(quest);

        QuestCompleted?.Invoke(quest);
        QuestListChanged?.Invoke();
        Debug.Log($"Задание выполнено: {quest.title}. Награда: {quest.rewardSummary}");
    }

    private QuestDefinition FindDefinition(string questId)
    {
        foreach (QuestDefinition definition in GetActiveDefinitions())
        {
            if (definition != null && definition.QuestId == questId)
                return definition;
        }

        return null;
    }

    private QuestWorldContext BuildContext(QuestTriggerType triggerType)
    {
        int totalRailTiles = 0;

        if (RailManager.Instance != null)
        {
            foreach (RailLine line in RailManager.Instance.Lines)
            {
                if (line != null)
                    totalRailTiles += line.Length;
            }
        }

        return new QuestWorldContext
        {
            TriggerType = triggerType,
            Day = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0,
            Hour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0,
            RailLineCount = RailManager.Instance != null ? RailManager.Instance.Lines.Count : 0,
            TotalRailTiles = totalRailTiles,
            TrainCount = TrainManager.Instance != null ? TrainManager.Instance.Trains.Count : 0,
            Balance = FinanceSystem.Instance != null ? FinanceSystem.Instance.Balance : 0f
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
}
