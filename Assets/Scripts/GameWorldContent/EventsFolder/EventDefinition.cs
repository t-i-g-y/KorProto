using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventTriggerType
{
    Manual,
    PathBuilt,
    PathRemoved,
    PathBuiltWithLength,
    TrainTravelledPathTimes,
    TrainDeliveredCargo,
    TrainArrivedAtStation,
    TrainDestroyed,
    FinanceAmountReached,
    IncomeEarnedForPeriod,
    AmountSpent,
    StationPopulationDecreased,
    StationPopulationIncreased,
    StationsConnected,
    ArtifactObtained,
    QuestCompleted,
    TechnologyUnlocked,
    SpecificDayReached,
    SpecificHourReached,
    TimePassed,
    RandomEvent,
    PathPassesThroughBiome,
    TrainCount,
    StationCount
}

public enum GameEventTimeUnit
{
    Hours,
    Days
}

public enum GameEventStationParameterMode
{
    Count,
    StationPair
}

public enum GameEventConsequenceMode
{
    Fixed,
    PlayerChoice
}

public enum GameEventEffectType
{
    None,
    AddBalance,
    SubtractBalance,
    AddResearchPoints,
    RemovePath,
    RemoveTrain,
    ChangeTrainSpeed,
    AddStationProducedResource,
    AddStationRequiredResource,
    AddStationPopulation,
    SubtractStationPopulation
}

public class EventWorldContext
{
    public GameEventTriggerType TriggerType;
    public int Day;
    public int Hour;
    public int RailLineCount;
    public int RemovedRailLineCount;
    public int TotalConnectedStationCount;
    public int TrainCount;
    public int TrainDestroyedCount;
    public int PathPassCount;
    public int CargoAmount;
    public ResourceType CargoType;
    public string StationName;
    public string StartStationName;
    public string EndStationName;
    public string DestinationStationName;
    public int ArrivalCount;
    public int PopulationDelta;
    public int ArtifactCount;
    public int QuestCompletedCount;
    public bool TechnologyUnlocked;
    public float IncomeAmount;
    public float SpentAmount;
    public int PeriodHours;
    public int HoursPassed;
    public float Balance;
    public TerrainType BiomeType;
    public RailLine RailLine;
    public Train Train;
}

[Serializable]
public class GameEventTriggerParameters
{
    [SerializeField] private int count = 1;
    [SerializeField] private GameEventStationParameterMode stationParameterMode = GameEventStationParameterMode.Count;
    [SerializeField] private string startStationName;
    [SerializeField] private string endStationName;
    [SerializeField] private int tileCount = 1;
    [SerializeField] private int passCount = 1;
    [SerializeField] private ResourceType cargoType = ResourceType.Coal;
    [SerializeField] private int cargoCount = 1;
    [SerializeField] private string destinationStationName;
    [SerializeField] private string stationName;
    [SerializeField] private int arrivalCount = 1;
    [SerializeField] private float amount;
    [SerializeField] private int timePeriodHours = 24;
    [SerializeField] private int populationCount = 1;
    [SerializeField] private int dayNumber = 1;
    [Range(0, 23)]
    [SerializeField] private int hour;
    [SerializeField] private int elapsedTimeAmount = 1;
    [SerializeField] private GameEventTimeUnit elapsedTimeUnit = GameEventTimeUnit.Days;
    [SerializeField] private int checkIntervalHours = 24;
    [Range(0f, 1f)]
    [SerializeField] private float randomEventChance = 1f;
    [SerializeField] private TerrainType biomeType = TerrainType.Grassland;
    [SerializeField] private int pathLength = 1;
    [SerializeField] private int trainBuiltCount = 1;
    [SerializeField] private int stationConnectedCount = 1;

    public int Count => Mathf.Max(0, count);
    public GameEventStationParameterMode StationParameterMode => stationParameterMode;
    public string StartStationName => startStationName;
    public string EndStationName => endStationName;
    public int TileCount => Mathf.Max(0, tileCount);
    public int PassCount => Mathf.Max(0, passCount);
    public ResourceType CargoType => cargoType;
    public int CargoCount => Mathf.Max(0, cargoCount);
    public string DestinationStationName => destinationStationName;
    public string StationName => stationName;
    public int ArrivalCount => Mathf.Max(0, arrivalCount);
    public float Amount => amount;
    public int TimePeriodHours => Mathf.Max(1, timePeriodHours);
    public int PopulationCount => Mathf.Max(0, populationCount);
    public int DayNumber => Mathf.Max(0, dayNumber);
    public int Hour => Mathf.Clamp(hour, 0, 23);
    public int ElapsedTimeHours => Mathf.Max(0, elapsedTimeAmount) * (elapsedTimeUnit == GameEventTimeUnit.Days ? 24 : 1);
    public int CheckIntervalHours => Mathf.Max(1, checkIntervalHours);
    public float RandomEventChance => Mathf.Clamp01(randomEventChance);
    public TerrainType BiomeType => biomeType;
    public int PathLength => Mathf.Max(0, pathLength);
    public int TrainBuiltCount => Mathf.Max(0, trainBuiltCount);
    public int StationConnectedCount => Mathf.Max(0, stationConnectedCount);

    public void SetCount(int value)
    {
        count = Mathf.Max(0, value);
        tileCount = Mathf.Max(0, value);
        passCount = Mathf.Max(0, value);
        cargoCount = Mathf.Max(0, value);
        arrivalCount = Mathf.Max(0, value);
        populationCount = Mathf.Max(0, value);
        trainBuiltCount = Mathf.Max(0, value);
        stationConnectedCount = Mathf.Max(0, value);
        dayNumber = Mathf.Max(0, value);
        elapsedTimeAmount = Mathf.Max(0, value);
    }

    public void SetAmount(float value)
    {
        amount = value;
    }

    public void SetCheckIntervalHours(int value)
    {
        checkIntervalHours = Mathf.Max(1, value);
    }
}

[Serializable]
public class GameEventEffect
{
    [SerializeField] private GameEventEffectType effectType;
    [SerializeField] private float floatAmount;
    [SerializeField] private int intAmount;
    [SerializeField] private ResourceType resourceType;
    [SerializeField] private string stationName;

    public GameEventEffect()
    {
    }

    public GameEventEffect(GameEventEffectType effectType, float floatAmount = 0f, int intAmount = 0, ResourceType resourceType = ResourceType.Coal)
    {
        this.effectType = effectType;
        this.floatAmount = floatAmount;
        this.intAmount = intAmount;
        this.resourceType = resourceType;
    }

    public GameEventEffectType EffectType => effectType;
    public float FloatAmount => floatAmount;
    public int IntAmount => intAmount;
    public ResourceType ResourceType => resourceType;

    public void Apply(EventWorldContext context)
    {
        switch (effectType)
        {
            case GameEventEffectType.AddBalance:
                FinanceSystem.Instance?.AdjustBalance(Mathf.Abs(floatAmount));
                break;
            case GameEventEffectType.SubtractBalance:
                FinanceSystem.Instance?.AdjustBalance(-Mathf.Abs(floatAmount));
                break;
            case GameEventEffectType.AddResearchPoints:
                ResearchIncomeSystem.Instance?.AddGlobalResearchPerHour(Mathf.Max(0, intAmount));
                break;
            case GameEventEffectType.RemoveTrain:
                TryRemoveTrain(context);
                break;
            case GameEventEffectType.RemovePath:
                TryRemoveRailLine(context);
                break;
            case GameEventEffectType.ChangeTrainSpeed:
                ChangeTrainSpeed(context);
                break;
            case GameEventEffectType.AddStationProducedResource:
                TryChangeStationResource(context, false);
                break;
            case GameEventEffectType.AddStationRequiredResource:
                TryChangeStationResource(context, true);
                break;
            case GameEventEffectType.AddStationPopulation:
                TryChangeStationPopulation(context, Mathf.Abs(intAmount));
                break;
            case GameEventEffectType.SubtractStationPopulation:
                TryChangeStationPopulation(context, -Mathf.Abs(intAmount));
                break;
        }
    }

    public string BuildSummary()
    {
        return effectType switch
        {
            GameEventEffectType.AddBalance => $"+{Mathf.Abs(floatAmount):0} к бюджету",
            GameEventEffectType.SubtractBalance => $"-{Mathf.Abs(floatAmount):0} к бюджету",
            GameEventEffectType.AddResearchPoints => $"+{Mathf.Max(0, intAmount)} очков исследования",
            GameEventEffectType.RemovePath => "удаление затронутого пути и поездов на нем",
            GameEventEffectType.RemoveTrain => "удаление случайного поезда",
            GameEventEffectType.ChangeTrainSpeed => $"скорость поездов x{floatAmount:0.##}",
            GameEventEffectType.AddStationProducedResource => $"+{Mathf.Max(0, intAmount)} производимого ресурса {resourceType}",
            GameEventEffectType.AddStationRequiredResource => $"+{Mathf.Max(0, intAmount)} необходимого ресурса {resourceType}",
            GameEventEffectType.AddStationPopulation => $"+{Mathf.Abs(intAmount)} населения на станции",
            GameEventEffectType.SubtractStationPopulation => $"-{Mathf.Abs(intAmount)} населения на станции",
            _ => string.Empty
        };
    }

    private static void TryRemoveTrain(EventWorldContext context)
    {
        EventManager.Instance?.RemoveLastCreatedTrain();
    }

    private static void TryRemoveRailLine(EventWorldContext context)
    {
        EventManager.Instance?.RemoveLastCreatedRailLine();
    }

    private void ChangeTrainSpeed(EventWorldContext context)
    {
        float multiplier = floatAmount;
        if (multiplier <= 0f)
            return;

        EventManager.Instance?.ApplyTrainSpeedEventMultiplier(multiplier);
    }

    private void TryChangeStationResource(EventWorldContext context, bool required)
    {
        Station station = FindStationByName(stationName, context);
        if (station == null)
            return;

        int amount = Mathf.Max(0, intAmount);
        if (required)
            station.AddConsumedResource(resourceType, amount);
        else
            station.AddProducedResource(resourceType, amount);
    }

    private void TryChangeStationPopulation(EventWorldContext context, int amount)
    {
        Station station = FindStationByName(stationName, context);
        if (station == null || amount == 0)
            return;

        if (amount > 0)
            station.IncreasePopulation(amount);
        else
            station.DecreasePopulation(-amount);
    }

    private static Station FindStationByName(string name, EventWorldContext context)
    {
        Station[] stations = UnityEngine.Object.FindObjectsByType<Station>(FindObjectsSortMode.None);
        if (stations == null || stations.Length == 0)
            return null;

        string targetName = !string.IsNullOrWhiteSpace(name) ? name : context?.StationName;
        if (string.IsNullOrWhiteSpace(targetName))
            return null;

        foreach (Station station in stations)
        {
            if (station == null)
                continue;

            if (string.Equals(station.StationName.Trim(), targetName.Trim(), StringComparison.OrdinalIgnoreCase))
                return station;
        }

        return null;
    }
}

[Serializable]
public class GameEventOption
{
    [SerializeField] private string title = "Последствие";
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private List<GameEventEffect> effects = new();

    public GameEventOption()
    {
    }

    public GameEventOption(string title, string description, params GameEventEffect[] effects)
    {
        this.title = title;
        this.description = description;
        this.effects = effects != null ? new List<GameEventEffect>(effects) : new List<GameEventEffect>();
    }

    public string Title => title;
    public string Description => description;
    public IReadOnlyList<GameEventEffect> Effects => effects;

    public void Apply(EventWorldContext context)
    {
        foreach (GameEventEffect effect in effects)
            effect?.Apply(context);
    }

    public string BuildEffectSummary()
    {
        List<string> parts = new();

        foreach (GameEventEffect effect in effects)
        {
            if (effect == null)
                continue;

            string summary = effect.BuildSummary();
            if (!string.IsNullOrWhiteSpace(summary))
                parts.Add(summary);
        }

        return parts.Count > 0 ? string.Join(", ", parts) : description;
    }
}

[CreateAssetMenu(menuName = "GameContent/Events/Event Definition")]
public class EventDefinition : ScriptableObject
{
    [Header("Event")]
    [SerializeField] private string eventId;
    [SerializeField] private string title = "Событие";
    [TextArea]
    [SerializeField] private string description;

    [Header("Trigger")]
    [SerializeField] private GameEventTriggerType triggerType = GameEventTriggerType.Manual;
    [SerializeField] private GameEventTriggerParameters triggerParameters = new();
    [Range(0f, 1f)]
    [SerializeField] private float chance = 1f;
    [SerializeField] private int minDay;
    [SerializeField] private bool canRepeat;
    [SerializeField] private int cooldownDays;

    [Header("Consequences")]
    [SerializeField] private GameEventConsequenceMode consequenceMode = GameEventConsequenceMode.Fixed;
    [SerializeField] private GameEventOption fixedConsequence = new();
    [SerializeField] private List<GameEventOption> choiceConsequences = new();

    public string EventId => string.IsNullOrWhiteSpace(eventId) ? name : eventId;
    public string Title => title;
    public string Description => description;
    public GameEventTriggerType TriggerType => triggerType;
    public GameEventTriggerParameters TriggerParameters => triggerParameters;
    public float Chance => chance;
    public int MinDay => minDay;
    public bool CanRepeat => canRepeat;
    public int CooldownDays => cooldownDays;
    public GameEventConsequenceMode ConsequenceMode => consequenceMode;
    public GameEventOption FixedConsequence => fixedConsequence;
    public IReadOnlyList<GameEventOption> ChoiceConsequences => choiceConsequences;

    public bool Matches(EventWorldContext context)
    {
        if (context == null || context.TriggerType != triggerType)
            return false;

        if (context.Day < minDay)
            return false;

        triggerParameters ??= new GameEventTriggerParameters();

        return triggerType switch
        {
            GameEventTriggerType.Manual => true,
            GameEventTriggerType.PathBuilt => MatchesPathCounter(context.RailLineCount, context),
            GameEventTriggerType.PathRemoved => MatchesPathCounter(context.RemovedRailLineCount, context),
            GameEventTriggerType.PathBuiltWithLength => context.RailLine != null && context.RailLine.Length >= triggerParameters.TileCount,
            GameEventTriggerType.TrainTravelledPathTimes => context.PathPassCount >= triggerParameters.PassCount,
            GameEventTriggerType.TrainDeliveredCargo => context.CargoType == triggerParameters.CargoType
                && context.CargoAmount >= triggerParameters.CargoCount
                && MatchesOptionalStationName(context.DestinationStationName, triggerParameters.DestinationStationName),
            GameEventTriggerType.TrainArrivedAtStation => context.ArrivalCount >= triggerParameters.ArrivalCount
                && MatchesOptionalStationName(context.StationName, triggerParameters.StationName),
            GameEventTriggerType.TrainDestroyed => context.TrainDestroyedCount >= triggerParameters.Count,
            GameEventTriggerType.FinanceAmountReached => context.Balance >= triggerParameters.Amount,
            GameEventTriggerType.IncomeEarnedForPeriod => context.IncomeAmount >= triggerParameters.Amount
                && context.PeriodHours >= triggerParameters.TimePeriodHours,
            GameEventTriggerType.AmountSpent => context.SpentAmount >= triggerParameters.Amount,
            GameEventTriggerType.StationPopulationDecreased => -context.PopulationDelta >= triggerParameters.PopulationCount
                && MatchesOptionalStationName(context.StationName, triggerParameters.StationName),
            GameEventTriggerType.StationPopulationIncreased => context.PopulationDelta >= triggerParameters.PopulationCount
                && MatchesOptionalStationName(context.StationName, triggerParameters.StationName),
            GameEventTriggerType.StationsConnected => context.TotalConnectedStationCount >= triggerParameters.Count,
            GameEventTriggerType.ArtifactObtained => context.ArtifactCount >= triggerParameters.Count,
            GameEventTriggerType.QuestCompleted => context.QuestCompletedCount >= triggerParameters.Count,
            GameEventTriggerType.TechnologyUnlocked => context.TechnologyUnlocked,
            GameEventTriggerType.SpecificDayReached => context.Day >= triggerParameters.DayNumber,
            GameEventTriggerType.SpecificHourReached => context.Hour == triggerParameters.Hour,
            GameEventTriggerType.TimePassed => context.HoursPassed >= triggerParameters.ElapsedTimeHours,
            GameEventTriggerType.RandomEvent => context.HoursPassed > 0
                && context.HoursPassed % triggerParameters.CheckIntervalHours == 0
                && UnityEngine.Random.value <= triggerParameters.RandomEventChance,
            GameEventTriggerType.PathPassesThroughBiome => RailLinePassesThroughBiome(context.RailLine),
            GameEventTriggerType.TrainCount => context.TrainCount >= triggerParameters.TrainBuiltCount,
            GameEventTriggerType.StationCount => context.TotalConnectedStationCount >= triggerParameters.StationConnectedCount,
            _ => false
        };
    }

    private bool MatchesPathCounter(int count, EventWorldContext context)
    {
        if (triggerParameters.StationParameterMode == GameEventStationParameterMode.Count)
            return count >= triggerParameters.Count;

        return MatchesStationPair(context);
    }

    private bool MatchesStationPair(EventWorldContext context)
    {
        if (context == null)
            return false;

        bool direct = MatchesStationName(context.StartStationName, triggerParameters.StartStationName)
            && MatchesStationName(context.EndStationName, triggerParameters.EndStationName);
        bool reverse = MatchesStationName(context.StartStationName, triggerParameters.EndStationName)
            && MatchesStationName(context.EndStationName, triggerParameters.StartStationName);

        return direct || reverse;
    }

    private static bool MatchesOptionalStationName(string actualName, string expectedName)
    {
        return string.IsNullOrWhiteSpace(expectedName) || MatchesStationName(actualName, expectedName);
    }

    private static bool MatchesStationName(string actualName, string expectedName)
    {
        return !string.IsNullOrWhiteSpace(actualName)
            && !string.IsNullOrWhiteSpace(expectedName)
            && string.Equals(actualName.Trim(), expectedName.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private bool RailLinePassesThroughBiome(RailLine line)
    {
        if (line == null || HexRailNetwork.Instance == null || line.Length < triggerParameters.PathLength)
            return false;

        foreach (Vector3Int cell in line.Cells)
        {
            if (HexRailNetwork.Instance.GetTerrainType(cell) == triggerParameters.BiomeType)
                return true;
        }

        return false;
    }

    public bool RollChance()
    {
        return chance >= 1f || UnityEngine.Random.value <= chance;
    }

    public IReadOnlyList<GameEventOption> GetCurrentOptions()
    {
        if (consequenceMode == GameEventConsequenceMode.PlayerChoice)
            return choiceConsequences;

        return fixedConsequence != null ? new[] { fixedConsequence } : Array.Empty<GameEventOption>();
    }

    public static EventDefinition CreateRuntime(
        string id,
        string eventTitle,
        string eventDescription,
        GameEventTriggerType eventTriggerType,
        float eventTriggerValue,
        GameEventConsequenceMode eventConsequenceMode,
        bool repeat,
        int cooldown,
        params GameEventOption[] options)
    {
        EventDefinition definition = CreateInstance<EventDefinition>();
        definition.eventId = id;
        definition.title = eventTitle;
        definition.description = eventDescription;
        definition.triggerType = eventTriggerType;
        definition.triggerParameters = new GameEventTriggerParameters();
        definition.triggerParameters.SetCount(Mathf.RoundToInt(eventTriggerValue));
        definition.triggerParameters.SetAmount(eventTriggerValue);
        if (eventTriggerType == GameEventTriggerType.RandomEvent)
            definition.triggerParameters.SetCheckIntervalHours(Mathf.RoundToInt(eventTriggerValue * 24f));
        definition.consequenceMode = eventConsequenceMode;
        definition.canRepeat = repeat;
        definition.cooldownDays = cooldown;

        if (eventConsequenceMode == GameEventConsequenceMode.Fixed)
        {
            if (options != null && options.Length > 0)
                definition.fixedConsequence = options[0];
        }
        else if (options != null)
        {
            definition.choiceConsequences = new List<GameEventOption>(options);
        }

        return definition;
    }
}
