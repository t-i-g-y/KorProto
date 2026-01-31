using UnityEngine;
using System.Collections.Generic;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance { get; private set; }

    [SerializeField] private List<TechData> allTechs;
    [SerializeField] private TechData startingTech;

    private HashSet<TechData> researchedTechs = new HashSet<TechData>();
    private TechData currentResearch;
    private float researchProgress;

    public int ResearchPointsPerDay { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        ResearchPointsPerDay = 5;
    }

    private void Update()
    {
        ResearchPointsPerDay += Mathf.FloorToInt(1f * TimeManager.Instance.CustomDeltaTime);

        if (currentResearch != null)
        {
            researchProgress += TimeManager.Instance.CustomDeltaTime;
        }
    }

    public void StartResearch(TechData tech)
    {
        if (currentResearch != null || !tech.CanResearch(this) || ResearchPointsPerDay < tech.rpCost) 
            return;

        ResearchPointsPerDay -= tech.rpCost;
        currentResearch = tech;
        researchProgress = 0f;
    }

    private void CompleteResearch()
    {
        researchedTechs.Add(currentResearch);
        ApplyEffect(currentResearch.effect);
        currentResearch = null;
        researchProgress = 0f;
    }

    public bool IsResearched(TechData tech) => researchedTechs.Contains(tech);

    private void ApplyEffect(TechEffect effect)
    {
        switch (effect)
        {
            case TechEffect.LakeCrossingUnlock:
                break;
            case TechEffect.MountainTunnelUnlock:
                break;
        }
    }
}
