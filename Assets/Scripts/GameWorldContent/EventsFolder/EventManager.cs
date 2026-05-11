using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private const int MaxHistoryEntries = 3;

    [SerializeField] private List<EventDefinition> eventDefinitions = new();
    [SerializeField] private bool useBuiltInEventsWhenEmpty = true;

    private readonly List<EventDefinition> runtimeDefinitions = new();
    private readonly List<EventHistoryEntry> history = new();
    private readonly HashSet<string> firedNonRepeatableEvents = new();
    private readonly Dictionary<string, int> lastTriggerDayByEventId = new();

    private TimeManager boundTimeManager;
    private QuestManager boundQuestManager;
    private ResearchSystem boundResearchSystem;
    private int elapsedHoursSinceStart;
    private int removedRailLineCount;
    private int destroyedTrainCount;
    private GameEventRuntime pendingEvent;
    private float timeMultiplierBeforePendingEvent = 1f;
    private bool shouldResumeTimeAfterPendingEvent;
    private bool isEventNotificationOpen;

    public static EventManager Instance { get; private set; }
    public IReadOnlyList<EventHistoryEntry> History => history;
    public GameEventRuntime PendingEvent => pendingEvent;
    public bool IsEventNotificationOpen => isEventNotificationOpen;

    public event Action<GameEventRuntime> EventActivated;
    public event Action<EventHistoryEntry> EventRecorded;
    public event Action<GameEventRuntime> EventResolved;
    public event Action PendingEventChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null || FindAnyObjectByType<EventManager>() != null)
            return;

        GameObject managerObject = new GameObject("EventManager");
        managerObject.AddComponent<EventManager>();
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
        Train.TrainBroken += HandleTrainBroken;
        TryBindTimeManager();
        TryBindQuestManager();
        TryBindResearchSystem();
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        RailManager.LineRemoved -= HandleLineRemoved;
        TrainManager.TrainCreated -= HandleTrainCreated;
        Train.TrainBroken -= HandleTrainBroken;
        UnbindTimeManager();
        UnbindQuestManager();
        UnbindResearchSystem();
    }

    private void Update()
    {
        if (boundTimeManager == null)
            TryBindTimeManager();

        if (boundQuestManager == null)
            TryBindQuestManager();

        if (boundResearchSystem == null)
            TryBindResearchSystem();
    }

    public void TriggerManualEvent(EventDefinition definition)
    {
        if (definition == null)
            return;

        TryActivate(definition, BuildContext(GameEventTriggerType.Manual));
    }

    public void NotifyTrainTravelledPath(RailLine line, int passCount)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainTravelledPathTimes);
        context.RailLine = line;
        context.PathPassCount = passCount;
        EvaluateTrigger(GameEventTriggerType.TrainTravelledPathTimes, context);
    }

    public void NotifyTrainDeliveredCargo(ResourceType cargoType, int amount)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainDeliveredCargo);
        context.CargoType = cargoType;
        context.CargoAmount = amount;
        EvaluateTrigger(GameEventTriggerType.TrainDeliveredCargo, context);
    }

    public void NotifyTrainDeliveredCargo(ResourceType cargoType, int amount, Station destinationStation)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainDeliveredCargo);
        context.CargoType = cargoType;
        context.CargoAmount = amount;
        context.DestinationStationName = destinationStation != null ? destinationStation.StationName : null;
        EvaluateTrigger(GameEventTriggerType.TrainDeliveredCargo, context);
    }

    public void NotifyTrainArrivedAtStation(int arrivalCount)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainArrivedAtStation);
        context.ArrivalCount = arrivalCount;
        EvaluateTrigger(GameEventTriggerType.TrainArrivedAtStation, context);
    }

    public void NotifyTrainArrivedAtStation(Station station, int arrivalCount)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainArrivedAtStation);
        context.StationName = station != null ? station.StationName : null;
        context.ArrivalCount = arrivalCount;
        EvaluateTrigger(GameEventTriggerType.TrainArrivedAtStation, context);
    }

    public void NotifyTrainDestroyed(Train train)
    {
        destroyedTrainCount++;

        EventWorldContext context = BuildContext(GameEventTriggerType.TrainDestroyed);
        context.Train = train;
        context.TrainDestroyedCount = destroyedTrainCount;
        EvaluateTrigger(GameEventTriggerType.TrainDestroyed, context);
    }

    public void NotifyIncomeEarned(float amount, int periodHours)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.IncomeEarnedForPeriod);
        context.IncomeAmount = amount;
        context.PeriodHours = periodHours;
        EvaluateTrigger(GameEventTriggerType.IncomeEarnedForPeriod, context);
    }

    public void NotifyAmountSpent(float amount)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.AmountSpent);
        context.SpentAmount = amount;
        EvaluateTrigger(GameEventTriggerType.AmountSpent, context);
    }

    public void NotifyStationPopulationChanged(int delta)
    {
        GameEventTriggerType triggerType = delta < 0
            ? GameEventTriggerType.StationPopulationDecreased
            : GameEventTriggerType.StationPopulationIncreased;

        EventWorldContext context = BuildContext(triggerType);
        context.PopulationDelta = delta;
        EvaluateTrigger(triggerType, context);
    }

    public void NotifyStationPopulationChanged(Station station, int delta)
    {
        GameEventTriggerType triggerType = delta < 0
            ? GameEventTriggerType.StationPopulationDecreased
            : GameEventTriggerType.StationPopulationIncreased;

        EventWorldContext context = BuildContext(triggerType);
        context.StationName = station != null ? station.StationName : null;
        context.PopulationDelta = delta;
        EvaluateTrigger(triggerType, context);
    }

    public void NotifyArtifactObtained(int count)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.ArtifactObtained);
        context.ArtifactCount = count;
        EvaluateTrigger(GameEventTriggerType.ArtifactObtained, context);
    }

    public void NotifyQuestCompleted(int count)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.QuestCompleted);
        context.QuestCompletedCount = count;
        EvaluateTrigger(GameEventTriggerType.QuestCompleted, context);
    }

    public void NotifyTechnologyUnlocked()
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TechnologyUnlocked);
        context.TechnologyUnlocked = true;
        EvaluateTrigger(GameEventTriggerType.TechnologyUnlocked, context);
    }

    public bool ResolvePendingChoice(int optionIndex)
    {
        if (pendingEvent == null || !pendingEvent.IsAwaitingChoice)
            return false;

        IReadOnlyList<GameEventOption> options = pendingEvent.Definition.GetCurrentOptions();
        if (optionIndex < 0 || optionIndex >= options.Count)
            return false;

        GameEventOption selectedOption = options[optionIndex];
        selectedOption.Apply(pendingEvent.Context);
        pendingEvent.SelectedOption = selectedOption;
        pendingEvent.IsAwaitingChoice = false;

        RecordEvent(pendingEvent);
        EventResolved?.Invoke(pendingEvent);

        Debug.Log(BuildLogMessage(pendingEvent));

        pendingEvent = null;
        PendingEventChanged?.Invoke();
        return true;
    }

    public void AcknowledgeEventNotification()
    {
        if (pendingEvent != null && pendingEvent.IsAwaitingChoice)
            return;

        isEventNotificationOpen = false;
        ResumeTimeAfterEventNotification();
    }

    public EventManagerSaveData GetSaveData()
    {
        EventManagerSaveData data = new();
        data.history.AddRange(history);

        foreach (string eventId in firedNonRepeatableEvents)
            data.firedEventIds.Add(eventId);

        foreach (KeyValuePair<string, int> pair in lastTriggerDayByEventId)
            data.cooldowns.Add(new EventCooldownSaveData(pair.Key, pair.Value));

        return data;
    }

    public void LoadFromSaveData(EventManagerSaveData data)
    {
        history.Clear();
        firedNonRepeatableEvents.Clear();
        lastTriggerDayByEventId.Clear();
        pendingEvent = null;
        shouldResumeTimeAfterPendingEvent = false;
        isEventNotificationOpen = false;

        if (data == null)
        {
            PendingEventChanged?.Invoke();
            return;
        }

        history.AddRange(data.history);
        TrimHistory();

        foreach (string eventId in data.firedEventIds)
        {
            if (!string.IsNullOrWhiteSpace(eventId))
                firedNonRepeatableEvents.Add(eventId);
        }

        foreach (EventCooldownSaveData cooldown in data.cooldowns)
        {
            if (!string.IsNullOrWhiteSpace(cooldown.eventId))
                lastTriggerDayByEventId[cooldown.eventId] = cooldown.lastTriggerDay;
        }

        PendingEventChanged?.Invoke();
    }

    private void BuildRuntimeDefinitions()
    {
        runtimeDefinitions.Clear();

        if (!useBuiltInEventsWhenEmpty)
            return;

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_first_line",
            "Первый железнодорожный контракт",
            "Новый путь сразу привлек внимание местных заказчиков.",
            GameEventTriggerType.PathBuilt,
            0f,
            GameEventConsequenceMode.Fixed,
            false,
            0,
            CreateRuntimeOption("Премия за запуск", "Бюджет пополнен за первый работающий участок.", GameEventEffectType.AddBalance, 25f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_inspection",
            "Проверка путевого хозяйства",
            "Инспекторы требуют решить, как обслуживать растущую сеть.",
            GameEventTriggerType.PathBuilt,
            3f,
            GameEventConsequenceMode.PlayerChoice,
            true,
            5,
            CreateRuntimeOption("Провести обслуживание", "Потратить средства сейчас и получить инженерный опыт.", GameEventEffectType.SubtractBalance, 20f, 0, GameEventEffectType.AddResearchPoints, 0f, 8),
            CreateRuntimeOption("Отложить работы", "Сохранить деньги, но снизить скорость поездов из-за ограничений.", GameEventEffectType.ChangeTrainSpeed, 0.9f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_breakdown_report",
            "Поломка состава",
            "Бригада докладывает о неисправности поезда на линии.",
            GameEventTriggerType.TrainDestroyed,
            0f,
            GameEventConsequenceMode.PlayerChoice,
            true,
            1,
            CreateRuntimeOption("Срочный ремонт", "Заплатить за ремонт затронутого поезда.", GameEventEffectType.SubtractBalance, 10f, 0),
            CreateRuntimeOption("Поставить в очередь", "Оставить поезд сломанным и не тратить бюджет прямо сейчас.", GameEventEffectType.None, 0f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_route_permit_dispute_4",
            "Владелец земли затеял спор!",
            "После прокладки длинного участка владельцы земли требуют компенсацию. Можно заплатить и оставить путь или разобрать спорный участок.",
            GameEventTriggerType.PathBuilt,
            4f,
            GameEventConsequenceMode.PlayerChoice,
            false,
            0,
            CreateRuntimeOption("Заплатить компенсацию", "Путь остается в сети, но бюджет уменьшается.", GameEventEffectType.SubtractBalance, 35f, 0),
            CreateRuntimeOption("Удалить путь", "Спорный путь демонтирован вместе с поездами на нем.", GameEventEffectType.RemovePath, 0f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_route_permit_dispute_8",
            "Владелец земли затеял спор!",
            "Расширение сети снова вызвало претензии владельцев земли. Можно заплатить и оставить путь или разобрать спорный участок.",
            GameEventTriggerType.PathBuilt,
            8f,
            GameEventConsequenceMode.PlayerChoice,
            false,
            0,
            CreateRuntimeOption("Заплатить компенсацию", "Путь остается в сети, но бюджет уменьшается.", GameEventEffectType.SubtractBalance, 35f, 0),
            CreateRuntimeOption("Удалить путь", "Спорный путь демонтирован вместе с поездами на нем.", GameEventEffectType.RemovePath, 0f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_city_request",
            "Городской заказ",
            "Одна из станций стала популярной, и местные власти просят обеспечить дополнительный спрос.",
            GameEventTriggerType.RandomEvent,
            4f,
            GameEventConsequenceMode.Fixed,
            true,
            4,
            CreateRuntimeOption("Спрос вырос", "На случайной станции появился дополнительный спрос.", GameEventEffectType.AddStationRequiredResource, 0f, 3)));
    }

    private static GameEventOption CreateRuntimeOption(
        string title,
        string description,
        GameEventEffectType effectType,
        float floatAmount,
        int intAmount,
        GameEventEffectType secondEffectType = GameEventEffectType.None,
        float secondFloatAmount = 0f,
        int secondIntAmount = 0)
    {
        List<GameEventEffect> effects = new()
        {
            CreateRuntimeEffect(effectType, floatAmount, intAmount)
        };

        if (secondEffectType != GameEventEffectType.None)
            effects.Add(CreateRuntimeEffect(secondEffectType, secondFloatAmount, secondIntAmount));

        return new GameEventOption(title, description, effects.ToArray());
    }

    private static GameEventEffect CreateRuntimeEffect(GameEventEffectType effectType, float floatAmount, int intAmount)
    {
        return new GameEventEffect(effectType, floatAmount, intAmount);
    }

    private IEnumerable<EventDefinition> GetActiveDefinitions()
    {
        bool hasConfiguredDefinitions = false;

        foreach (EventDefinition definition in eventDefinitions)
        {
            if (definition == null)
                continue;

            hasConfiguredDefinitions = true;
            yield return definition;
        }

        if (hasConfiguredDefinitions || !useBuiltInEventsWhenEmpty)
            yield break;

        foreach (EventDefinition definition in runtimeDefinitions)
            yield return definition;
    }

    private void HandleHourChanged(int day, int hour)
    {
        elapsedHoursSinceStart++;

        EventWorldContext hourContext = BuildContext(GameEventTriggerType.SpecificHourReached);
        hourContext.HoursPassed = elapsedHoursSinceStart;
        EvaluateTrigger(GameEventTriggerType.SpecificHourReached, hourContext);

        EventWorldContext timePassedContext = BuildContext(GameEventTriggerType.TimePassed);
        timePassedContext.HoursPassed = elapsedHoursSinceStart;
        EvaluateTrigger(GameEventTriggerType.TimePassed, timePassedContext);

        EventWorldContext randomContext = BuildContext(GameEventTriggerType.RandomEvent);
        randomContext.HoursPassed = elapsedHoursSinceStart;
        EvaluateTrigger(GameEventTriggerType.RandomEvent, randomContext);

        EvaluateTrigger(GameEventTriggerType.FinanceAmountReached, BuildContext(GameEventTriggerType.FinanceAmountReached));
    }

    private void HandleDayChanged(int day)
    {
        EvaluateTrigger(GameEventTriggerType.SpecificDayReached, BuildContext(GameEventTriggerType.SpecificDayReached));
    }

    private void HandleLineCreated(RailLine line)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.PathBuilt);
        context.RailLine = line;
        FillRailEndpointStationNames(context, line);
        EvaluateTrigger(GameEventTriggerType.PathBuilt, context);

        context.TriggerType = GameEventTriggerType.PathBuiltWithLength;
        EvaluateTrigger(GameEventTriggerType.PathBuiltWithLength, context);

        context.TriggerType = GameEventTriggerType.PathPassesThroughBiome;
        EvaluateTrigger(GameEventTriggerType.PathPassesThroughBiome, context);

        context.TriggerType = GameEventTriggerType.StationsConnected;
        EvaluateTrigger(GameEventTriggerType.StationsConnected, context);

        context.TriggerType = GameEventTriggerType.StationCount;
        EvaluateTrigger(GameEventTriggerType.StationCount, context);
    }

    private void HandleLineRemoved(RailLine line)
    {
        removedRailLineCount++;

        EventWorldContext context = BuildContext(GameEventTriggerType.PathRemoved);
        context.RailLine = line;
        context.RemovedRailLineCount = removedRailLineCount;
        FillRailEndpointStationNames(context, line);
        EvaluateTrigger(GameEventTriggerType.PathRemoved, context);
    }

    private void HandleTrainCreated(Train train, RailLine line)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainCount);
        context.Train = train;
        context.RailLine = line;
        EvaluateTrigger(GameEventTriggerType.TrainCount, context);
    }

    private void HandleTrainBroken(Train train)
    {
        NotifyTrainDestroyed(train);
    }

    private void HandleQuestCompleted(QuestRuntime quest)
    {
        NotifyQuestCompleted(boundQuestManager != null ? boundQuestManager.CompletedQuests.Count : 1);
    }

    private void HandleTechnologyUnlocked(Technology technology)
    {
        NotifyTechnologyUnlocked();
    }

    private void EvaluateTrigger(GameEventTriggerType triggerType, EventWorldContext context)
    {
        if (pendingEvent != null || isEventNotificationOpen)
            return;

        context.TriggerType = triggerType;

        foreach (EventDefinition definition in GetActiveDefinitions())
        {
            if (definition == null || definition.TriggerType != triggerType)
                continue;

            TryActivate(definition, context);

            if (pendingEvent != null)
                break;
        }
    }

    private bool TryActivate(EventDefinition definition, EventWorldContext context)
    {
        if (pendingEvent != null || isEventNotificationOpen)
            return false;

        if (!CanActivate(definition, context))
            return false;

        GameEventRuntime runtime = new(definition, CloneContext(context));
        MarkTriggered(definition, runtime.Context.Day);
        isEventNotificationOpen = true;
        PauseTimeForEventNotification();

        if (definition.ConsequenceMode == GameEventConsequenceMode.PlayerChoice)
        {
            IReadOnlyList<GameEventOption> options = definition.GetCurrentOptions();
            if (options.Count > 1)
            {
                runtime.IsAwaitingChoice = true;
                pendingEvent = runtime;
                EventActivated?.Invoke(runtime);
                PendingEventChanged?.Invoke();
                Debug.Log($"Событие: {definition.Title}. Требуется выбор последствия.");
                return true;
            }
        }

        GameEventOption option = definition.FixedConsequence;
        option?.Apply(runtime.Context);
        runtime.SelectedOption = option;
        RecordEvent(runtime);
        EventActivated?.Invoke(runtime);
        EventResolved?.Invoke(runtime);
        Debug.Log(BuildLogMessage(runtime));
        return true;
    }

    private bool CanActivate(EventDefinition definition, EventWorldContext context)
    {
        string eventId = definition.EventId;

        if (!definition.CanRepeat && firedNonRepeatableEvents.Contains(eventId))
            return false;

        if (lastTriggerDayByEventId.TryGetValue(eventId, out int lastDay))
        {
            int cooldown = Mathf.Max(0, definition.CooldownDays);
            if (cooldown > 0 && context.Day - lastDay < cooldown)
                return false;
        }

        return definition.Matches(context) && definition.RollChance();
    }

    private void MarkTriggered(EventDefinition definition, int day)
    {
        string eventId = definition.EventId;
        lastTriggerDayByEventId[eventId] = day;

        if (!definition.CanRepeat)
            firedNonRepeatableEvents.Add(eventId);
    }

    private void RecordEvent(GameEventRuntime runtime)
    {
        EventHistoryEntry entry = EventHistoryEntry.FromRuntime(runtime);
        history.Add(entry);
        TrimHistory();

        EventRecorded?.Invoke(entry);
    }

    private void TrimHistory()
    {
        while (history.Count > MaxHistoryEntries)
            history.RemoveAt(0);
    }

    private EventWorldContext BuildContext(GameEventTriggerType triggerType)
    {
        return new EventWorldContext
        {
            TriggerType = triggerType,
            Day = TimeManager.Instance != null ? TimeManager.Instance.DayCounter : 0,
            Hour = TimeManager.Instance != null ? TimeManager.Instance.HourCounter : 0,
            RailLineCount = RailManager.Instance != null ? RailManager.Instance.Lines.Count : 0,
            RemovedRailLineCount = removedRailLineCount,
            TotalConnectedStationCount = StationEconomySystem.Instance != null ? StationEconomySystem.Instance.Stations.Count : 0,
            TrainCount = TrainManager.Instance != null ? TrainManager.Instance.Trains.Count : 0,
            TrainDestroyedCount = destroyedTrainCount,
            HoursPassed = elapsedHoursSinceStart,
            Balance = FinanceSystem.Instance != null ? FinanceSystem.Instance.Balance : 0f
        };
    }

    private static EventWorldContext CloneContext(EventWorldContext context)
    {
        return new EventWorldContext
        {
            TriggerType = context.TriggerType,
            Day = context.Day,
            Hour = context.Hour,
            RailLineCount = context.RailLineCount,
            RemovedRailLineCount = context.RemovedRailLineCount,
            TotalConnectedStationCount = context.TotalConnectedStationCount,
            TrainCount = context.TrainCount,
            TrainDestroyedCount = context.TrainDestroyedCount,
            PathPassCount = context.PathPassCount,
            CargoAmount = context.CargoAmount,
            CargoType = context.CargoType,
            StationName = context.StationName,
            StartStationName = context.StartStationName,
            EndStationName = context.EndStationName,
            DestinationStationName = context.DestinationStationName,
            ArrivalCount = context.ArrivalCount,
            PopulationDelta = context.PopulationDelta,
            ArtifactCount = context.ArtifactCount,
            QuestCompletedCount = context.QuestCompletedCount,
            TechnologyUnlocked = context.TechnologyUnlocked,
            IncomeAmount = context.IncomeAmount,
            SpentAmount = context.SpentAmount,
            PeriodHours = context.PeriodHours,
            HoursPassed = context.HoursPassed,
            Balance = context.Balance,
            BiomeType = context.BiomeType,
            RailLine = context.RailLine,
            Train = context.Train
        };
    }

    private static void FillRailEndpointStationNames(EventWorldContext context, RailLine line)
    {
        if (context == null || line == null)
            return;

        context.StartStationName = StationRegistry.TryGet(line.Start, out Station startStation) ? startStation.StationName : null;
        context.EndStationName = StationRegistry.TryGet(line.End, out Station endStation) ? endStation.StationName : null;
    }

    private void TryBindTimeManager()
    {
        if (boundTimeManager != null || TimeManager.Instance == null)
            return;

        boundTimeManager = TimeManager.Instance;
        boundTimeManager.OnHourChanged += HandleHourChanged;
        boundTimeManager.OnDayChanged += HandleDayChanged;
    }

    private void UnbindTimeManager()
    {
        if (boundTimeManager == null)
            return;

        boundTimeManager.OnHourChanged -= HandleHourChanged;
        boundTimeManager.OnDayChanged -= HandleDayChanged;
        boundTimeManager = null;
    }

    private void TryBindQuestManager()
    {
        if (boundQuestManager != null || QuestManager.Instance == null)
            return;

        boundQuestManager = QuestManager.Instance;
        boundQuestManager.QuestCompleted += HandleQuestCompleted;
    }

    private void UnbindQuestManager()
    {
        if (boundQuestManager == null)
            return;

        boundQuestManager.QuestCompleted -= HandleQuestCompleted;
        boundQuestManager = null;
    }

    private void TryBindResearchSystem()
    {
        if (boundResearchSystem != null || ResearchSystem.Instance == null)
            return;

        boundResearchSystem = ResearchSystem.Instance;
        boundResearchSystem.OnTechnologyUnlocked += HandleTechnologyUnlocked;
    }

    private void UnbindResearchSystem()
    {
        if (boundResearchSystem == null)
            return;

        boundResearchSystem.OnTechnologyUnlocked -= HandleTechnologyUnlocked;
        boundResearchSystem = null;
    }

    private void PauseTimeForEventNotification()
    {
        if (TimeManager.Instance == null)
            return;

        timeMultiplierBeforePendingEvent = TimeManager.Instance.TimeMultiplier;
        shouldResumeTimeAfterPendingEvent = timeMultiplierBeforePendingEvent > 0f;
        TimeManager.Instance.Pause();
    }

    private void ResumeTimeAfterEventNotification()
    {
        if (!shouldResumeTimeAfterPendingEvent || TimeManager.Instance == null)
            return;

        if (MenuPauseState.IsPaused)
        {
            shouldResumeTimeAfterPendingEvent = false;
            return;
        }

        TimeManager.Instance.Unpause();
        shouldResumeTimeAfterPendingEvent = false;
    }

    private static string BuildLogMessage(GameEventRuntime runtime)
    {
        string consequence = runtime.SelectedOption != null
            ? $"{runtime.SelectedOption.Title}: {runtime.SelectedOption.Description}"
            : "последствие не выбрано";

        return $"Событие: {runtime.Definition.Title}. Последствие: {consequence}";
    }
}

public class GameEventRuntime
{
    public GameEventRuntime(EventDefinition definition, EventWorldContext context)
    {
        Definition = definition;
        Context = context;
    }

    public EventDefinition Definition { get; }
    public EventWorldContext Context { get; }
    public GameEventOption SelectedOption { get; set; }
    public bool IsAwaitingChoice { get; set; }
}
