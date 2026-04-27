using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameEventTriggerType
{
    Manual,
    DayReached,
    DayInterval,
    RailLineCreated,
    RailLineCountReached,
    RailLengthReached,
    TrainCreated,
    TrainCountReached,
    TrainBroken,
    BalanceBelow
}

public enum GameEventConsequenceMode
{
    Fixed,
    PlayerChoice
}

public enum GameEventEffectType
{
    None,
    AdjustBalance,
    AddResearchPoints,
    RepairContextTrain,
    RepairRandomBrokenTrain,
    RemoveRandomTrain,
    ChangeAllTrainSpeed,
    AddDemandToRandomStation,
    AddSupplyToRandomStation,
    IncreaseRandomStationPopulation,
    DecreaseRandomStationPopulation
}

public class EventWorldContext
{
    public GameEventTriggerType TriggerType;
    public int Day;
    public int Hour;
    public int RailLineCount;
    public int TrainCount;
    public float Balance;
    public RailLine RailLine;
    public Train Train;
}

[Serializable]
public class GameEventEffect
{
    [SerializeField] private GameEventEffectType effectType;
    [SerializeField] private float floatAmount;
    [SerializeField] private int intAmount;
    [SerializeField] private ResourceType resourceType;

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
            case GameEventEffectType.AdjustBalance:
                FinanceSystem.Instance?.AdjustBalance(floatAmount);
                break;
            case GameEventEffectType.AddResearchPoints:
                ResearchSystem.Instance?.AddResearchPoints(Mathf.Max(0, intAmount));
                break;
            case GameEventEffectType.RepairContextTrain:
                if (context != null && context.Train != null)
                    context.Train.Repair();
                break;
            case GameEventEffectType.RepairRandomBrokenTrain:
                TryRepairRandomBrokenTrain();
                break;
            case GameEventEffectType.RemoveRandomTrain:
                TryRemoveRandomTrain();
                break;
            case GameEventEffectType.ChangeAllTrainSpeed:
                ChangeAllTrainSpeed(floatAmount);
                break;
            case GameEventEffectType.AddDemandToRandomStation:
                TryChangeRandomStationResource(true);
                break;
            case GameEventEffectType.AddSupplyToRandomStation:
                TryChangeRandomStationResource(false);
                break;
            case GameEventEffectType.IncreaseRandomStationPopulation:
                TryChangeRandomStationPopulation(Mathf.Abs(intAmount));
                break;
            case GameEventEffectType.DecreaseRandomStationPopulation:
                TryChangeRandomStationPopulation(-Mathf.Abs(intAmount));
                break;
        }
    }

    public string BuildSummary()
    {
        return effectType switch
        {
            GameEventEffectType.AdjustBalance => floatAmount >= 0f ? $"+{floatAmount:0} к бюджету" : $"{floatAmount:0} к бюджету",
            GameEventEffectType.AddResearchPoints => $"+{Mathf.Max(0, intAmount)} очков исследования",
            GameEventEffectType.RepairContextTrain => "ремонт затронутого поезда",
            GameEventEffectType.RepairRandomBrokenTrain => "ремонт случайного сломанного поезда",
            GameEventEffectType.RemoveRandomTrain => "списание случайного поезда",
            GameEventEffectType.ChangeAllTrainSpeed => $"скорость всех поездов x{floatAmount:0.##}",
            GameEventEffectType.AddDemandToRandomStation => $"+{Mathf.Max(0, intAmount)} спроса на {resourceType}",
            GameEventEffectType.AddSupplyToRandomStation => $"+{Mathf.Max(0, intAmount)} запаса {resourceType}",
            GameEventEffectType.IncreaseRandomStationPopulation => $"+{Mathf.Abs(intAmount)} населения на станции",
            GameEventEffectType.DecreaseRandomStationPopulation => $"-{Mathf.Abs(intAmount)} населения на станции",
            _ => string.Empty
        };
    }

    private static void TryRepairRandomBrokenTrain()
    {
        if (TrainManager.Instance == null)
            return;

        List<Train> brokenTrains = TrainManager.Instance.GetBrokenTrains();
        if (brokenTrains.Count == 0)
            return;

        brokenTrains[UnityEngine.Random.Range(0, brokenTrains.Count)].Repair();
    }

    private static void TryRemoveRandomTrain()
    {
        if (TrainManager.Instance == null || TrainManager.Instance.Trains.Count == 0)
            return;

        int index = UnityEngine.Random.Range(0, TrainManager.Instance.Trains.Count);
        TrainManager.Instance.RemoveTrain(TrainManager.Instance.Trains[index]);
    }

    private static void ChangeAllTrainSpeed(float multiplier)
    {
        if (TrainManager.Instance == null || multiplier <= 0f)
            return;

        foreach (Train train in TrainManager.Instance.Trains)
        {
            if (train != null)
                train.ChangeSpeed(multiplier);
        }
    }

    private void TryChangeRandomStationResource(bool demand)
    {
        Station station = FindRandomStation();
        if (station == null)
            return;

        int amount = Mathf.Max(0, intAmount);
        if (demand)
            station.AddDemand(resourceType, amount);
        else
            station.AddSupply(resourceType, amount);
    }

    private void TryChangeRandomStationPopulation(int amount)
    {
        Station station = FindRandomStation();
        if (station == null || amount == 0)
            return;

        if (amount > 0)
            station.IncreasePopulation(amount);
        else
            station.DecreasePopulation(-amount);
    }

    private static Station FindRandomStation()
    {
        Station[] stations = UnityEngine.Object.FindObjectsByType<Station>(FindObjectsSortMode.None);
        if (stations == null || stations.Length == 0)
            return null;

        return stations[UnityEngine.Random.Range(0, stations.Length)];
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

[CreateAssetMenu(menuName = "Rail/Events/Event Definition")]
public class EventDefinition : ScriptableObject
{
    [Header("Event")]
    [SerializeField] private string eventId;
    [SerializeField] private string title = "Событие";
    [TextArea]
    [SerializeField] private string description;

    [Header("Trigger")]
    [SerializeField] private GameEventTriggerType triggerType = GameEventTriggerType.Manual;
    [SerializeField] private float triggerValue;
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
    public float TriggerValue => triggerValue;
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

        int threshold = Mathf.RoundToInt(triggerValue);

        return triggerType switch
        {
            GameEventTriggerType.Manual => true,
            GameEventTriggerType.DayReached => context.Day >= threshold,
            GameEventTriggerType.DayInterval => threshold > 0 && context.Day > 0 && context.Day % threshold == 0,
            GameEventTriggerType.RailLineCreated => true,
            GameEventTriggerType.RailLineCountReached => context.RailLineCount >= threshold,
            GameEventTriggerType.RailLengthReached => context.RailLine != null && context.RailLine.Length >= threshold,
            GameEventTriggerType.TrainCreated => true,
            GameEventTriggerType.TrainCountReached => context.TrainCount >= threshold,
            GameEventTriggerType.TrainBroken => context.Train != null,
            GameEventTriggerType.BalanceBelow => context.Balance <= triggerValue,
            _ => false
        };
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
        definition.triggerValue = eventTriggerValue;
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
