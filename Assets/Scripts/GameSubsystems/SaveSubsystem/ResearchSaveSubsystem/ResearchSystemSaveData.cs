using System;
using System.Collections.Generic;

[Serializable]
public class ResearchSystemSaveData
{
    public int currentResearchID;
    public List<TechnologySaveData> technologies = new();
}