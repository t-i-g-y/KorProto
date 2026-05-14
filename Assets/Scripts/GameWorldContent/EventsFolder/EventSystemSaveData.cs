using System;
using System.Collections.Generic;

[Serializable]
public class EventHistoryEntry
{
    public string eventId;
    public string title;
    public string description;
    public string consequenceTitle;
    public string consequenceDescription;
    public string consequenceEffects;
    public int day;
    public int hour;
    public int triggerType;

    public static EventHistoryEntry FromRuntime(GameEventRuntime runtime)
    {
        GameEventOption option = runtime.SelectedOption;

        return new EventHistoryEntry
        {
            eventId = runtime.Definition.EventId,
            title = runtime.Definition.Title,
            description = runtime.Definition.Description,
            consequenceTitle = option != null ? option.Title : string.Empty,
            consequenceDescription = option != null ? option.Description : string.Empty,
            consequenceEffects = option != null ? option.BuildEffectSummary() : string.Empty,
            day = runtime.Context.Day,
            hour = runtime.Context.Hour,
            triggerType = (int)runtime.Context.TriggerType
        };
    }
}

[Serializable]
public class EventCooldownSaveData
{
    public string eventId;
    public int lastTriggerDay;

    public EventCooldownSaveData()
    {
    }

    public EventCooldownSaveData(string eventId, int lastTriggerDay)
    {
        this.eventId = eventId;
        this.lastTriggerDay = lastTriggerDay;
    }
}

[Serializable]
public class EventManagerSaveData
{
    public List<EventHistoryEntry> history = new();
    public List<string> firedEventIds = new();
    public List<EventCooldownSaveData> cooldowns = new();
}
