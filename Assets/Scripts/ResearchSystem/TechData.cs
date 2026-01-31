using UnityEngine;

[CreateAssetMenu(fileName = "Tech", menuName = "Research/TechData")]
public class TechData : ScriptableObject
{
    public string techName;
    public int rpCost;
    public TechPrerequisite prerequisite;
    public TechEffect effect;

    public bool CanResearch(ResearchManager manager)
    {
        foreach (var prereq in prerequisite.requiredTech)
        {
            if (!manager.IsResearched(prereq)) 
                return false;
        }
        return true;
    }
}

public enum TechEffect
{
    None,
    TrainSpeedImprovement1,
    TrainSpeedImprovement2,
    LakeCrossingUnlock,
    MountainTunnelUnlock,
    SeaTunnelUnlock
}
