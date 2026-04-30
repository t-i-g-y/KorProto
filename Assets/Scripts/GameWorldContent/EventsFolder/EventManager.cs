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
    private GameEventRuntime pendingEvent;

    public static EventManager Instance { get; private set; }
    public IReadOnlyList<EventHistoryEntry> History => history;
    public GameEventRuntime PendingEvent => pendingEvent;

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
        TrainManager.TrainCreated += HandleTrainCreated;
        Train.TrainBroken += HandleTrainBroken;
        TryBindTimeManager();
    }

    private void OnDisable()
    {
        RailManager.LineCreated -= HandleLineCreated;
        TrainManager.TrainCreated -= HandleTrainCreated;
        Train.TrainBroken -= HandleTrainBroken;
        UnbindTimeManager();
    }

    private void Update()
    {
        if (boundTimeManager == null)
            TryBindTimeManager();
    }

    public void TriggerManualEvent(EventDefinition definition)
    {
        if (definition == null)
            return;

        TryActivate(definition, BuildContext(GameEventTriggerType.Manual));
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
            GameEventTriggerType.RailLineCreated,
            0f,
            GameEventConsequenceMode.Fixed,
            false,
            0,
            CreateRuntimeOption("Премия за запуск", "Бюджет пополнен за первый работающий участок.", GameEventEffectType.AdjustBalance, 25f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_inspection",
            "Проверка путевого хозяйства",
            "Инспекторы требуют решить, как обслуживать растущую сеть.",
            GameEventTriggerType.RailLineCountReached,
            3f,
            GameEventConsequenceMode.PlayerChoice,
            true,
            5,
            CreateRuntimeOption("Провести обслуживание", "Потратить средства сейчас и получить инженерный опыт.", GameEventEffectType.AdjustBalance, -20f, 0, GameEventEffectType.AddResearchPoints, 0f, 8),
            CreateRuntimeOption("Отложить работы", "Сохранить деньги, но снизить скорость поездов из-за ограничений.", GameEventEffectType.ChangeAllTrainSpeed, 0.9f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_breakdown_report",
            "Поломка состава",
            "Бригада докладывает о неисправности поезда на линии.",
            GameEventTriggerType.TrainBroken,
            0f,
            GameEventConsequenceMode.PlayerChoice,
            true,
            1,
            CreateRuntimeOption("Срочный ремонт", "Заплатить за ремонт затронутого поезда.", GameEventEffectType.AdjustBalance, -10f, 0, GameEventEffectType.RepairContextTrain, 0f, 0),
            CreateRuntimeOption("Поставить в очередь", "Оставить поезд сломанным и не тратить бюджет прямо сейчас.", GameEventEffectType.None, 0f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_route_permit_dispute_4",
            "Владелец земли затеял спор!",
            "После прокладки длинного участка владельцы земли требуют компенсацию. Можно заплатить и оставить путь или разобрать спорный участок.",
            GameEventTriggerType.RailLineCountReached,
            4f,
            GameEventConsequenceMode.PlayerChoice,
            false,
            0,
            CreateRuntimeOption("Заплатить компенсацию", "Путь остается в сети, но бюджет уменьшается.", GameEventEffectType.AdjustBalance, -35f, 0),
            CreateRuntimeOption("Удалить путь", "Спорный путь демонтирован вместе с поездами на нем.", GameEventEffectType.RemoveContextRailLine, 0f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_route_permit_dispute_8",
            "Владелец земли затеял спор!",
            "Расширение сети снова вызвало претензии владельцев земли. Можно заплатить и оставить путь или разобрать спорный участок.",
            GameEventTriggerType.RailLineCountReached,
            8f,
            GameEventConsequenceMode.PlayerChoice,
            false,
            0,
            CreateRuntimeOption("Заплатить компенсацию", "Путь остается в сети, но бюджет уменьшается.", GameEventEffectType.AdjustBalance, -35f, 0),
            CreateRuntimeOption("Удалить путь", "Спорный путь демонтирован вместе с поездами на нем.", GameEventEffectType.RemoveContextRailLine, 0f, 0)));

        runtimeDefinitions.Add(EventDefinition.CreateRuntime(
            "builtin_city_request",
            "Городской заказ",
            "Одна из станций стала популярной, и местные власти просят обеспечить дополнительный спрос.",
            GameEventTriggerType.DayInterval,
            4f,
            GameEventConsequenceMode.Fixed,
            true,
            4,
            CreateRuntimeOption("Спрос вырос", "На случайной станции появился дополнительный спрос.", GameEventEffectType.AddDemandToRandomStation, 0f, 3)));
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

    private void HandleDayChanged(int day)
    {
        EvaluateTrigger(GameEventTriggerType.DayReached, BuildContext(GameEventTriggerType.DayReached));
        EvaluateTrigger(GameEventTriggerType.DayInterval, BuildContext(GameEventTriggerType.DayInterval));
        EvaluateTrigger(GameEventTriggerType.BalanceBelow, BuildContext(GameEventTriggerType.BalanceBelow));
    }

    private void HandleLineCreated(RailLine line)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.RailLineCreated);
        context.RailLine = line;
        EvaluateTrigger(GameEventTriggerType.RailLineCreated, context);

        context.TriggerType = GameEventTriggerType.RailLengthReached;
        EvaluateTrigger(GameEventTriggerType.RailLengthReached, context);

        context.TriggerType = GameEventTriggerType.RailLineCountReached;
        EvaluateTrigger(GameEventTriggerType.RailLineCountReached, context);
    }

    private void HandleTrainCreated(Train train, RailLine line)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainCreated);
        context.Train = train;
        context.RailLine = line;
        EvaluateTrigger(GameEventTriggerType.TrainCreated, context);

        context.TriggerType = GameEventTriggerType.TrainCountReached;
        EvaluateTrigger(GameEventTriggerType.TrainCountReached, context);
    }

    private void HandleTrainBroken(Train train)
    {
        EventWorldContext context = BuildContext(GameEventTriggerType.TrainBroken);
        context.Train = train;
        context.RailLine = train != null ? train.AssignedLine : null;
        EvaluateTrigger(GameEventTriggerType.TrainBroken, context);
    }

    private void EvaluateTrigger(GameEventTriggerType triggerType, EventWorldContext context)
    {
        if (pendingEvent != null)
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
        if (!CanActivate(definition, context))
            return false;

        GameEventRuntime runtime = new(definition, CloneContext(context));
        MarkTriggered(definition, runtime.Context.Day);

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
            TrainCount = TrainManager.Instance != null ? TrainManager.Instance.Trains.Count : 0,
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
            TrainCount = context.TrainCount,
            Balance = context.Balance,
            RailLine = context.RailLine,
            Train = context.Train
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
