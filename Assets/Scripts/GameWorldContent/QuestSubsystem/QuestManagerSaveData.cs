using System;
using System.Collections.Generic;

[Serializable]
public class QuestManagerSaveData
{
    public List<QuestRuntime> activeQuests = new();
    public List<QuestRuntime> completedQuests = new();
    public List<string> activatedQuestIds = new();
}
