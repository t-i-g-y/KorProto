using System;
using System.Collections.Generic;
using UnityEngine;

public class ResearchSystem : MonoBehaviour
{
    public static ResearchSystem Instance { get; private set; }
    [SerializeField] private List<TechData> techDatabase = new();
    private readonly Dictionary<TechID, Technology> technologies = new();
    [SerializeField] private RailBuilderController railBuilder;
    private Technology currentResearch;
    public Technology CurrentResearch => currentResearch;
    public event Action OnResearchStateChanged;
    public event Action<Technology> OnTechnologyUnlocked;
    public event Action<Technology> OnResearchStarted;
    public event Action OnResearchCleared;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildRuntimeTechnologies();
    }

    private void BuildRuntimeTechnologies()
    {
        technologies.Clear();

        foreach (TechData data in techDatabase)
        {
            if (data == null)
                continue;

            if (technologies.ContainsKey(data.ID))
            {
                Debug.LogWarning($"Duplicate tech ID:{data.ID}");
                continue;
            }

            technologies.Add(data.ID, new Technology(data));
        }
    }

    public Dictionary<TechID, Technology> GetAllTechnologies()
    {
        return technologies;
    }

    public Technology GetTechnology(TechID id)
    {
        technologies.TryGetValue(id, out Technology tech);
        return tech;
    }

    public bool IsUnlocked(TechID id)
    {
        Technology tech = GetTechnology(id);
        return tech != null && tech.IsUnlocked;
    }

    public bool CanResearch(TechID id)
    {
        Technology tech = GetTechnology(id);

        if (tech == null)
            return false;

        if (tech.IsUnlocked)
            return false;

        foreach (TechID prereqId in tech.Data.prerequisites)
        {
            if (!IsUnlocked(prereqId))
                return false;
        }

        return true;
    }

    public bool StartResearch(TechID id)
    {
        Technology tech = GetTechnology(id);

        if (tech == null)
            return false;

        if (!CanResearch(id))
            return false;

        if (currentResearch != null)
            currentResearch.StopResearch();

        currentResearch = tech;
        currentResearch.StartResearch();

        OnResearchStarted?.Invoke(currentResearch);
        OnResearchStateChanged?.Invoke();
        return true;
    }

    public void StopResearch()
    {
        if (currentResearch == null)
            return;

        currentResearch.StopResearch();
        currentResearch = null;

        OnResearchCleared?.Invoke();
        OnResearchStateChanged?.Invoke();
    }

    public void AddResearchPoints(int amount)
    {
        if (amount <= 0)
            return;

        if (currentResearch == null)
            return;

        if (currentResearch.IsUnlocked)
            return;

        currentResearch.AddProgress(amount);

        if (currentResearch.IsComplete)
        {
            UnlockTechnology(currentResearch);
        }

        OnResearchStateChanged?.Invoke();
    }

    private void UnlockTechnology(Technology tech)
    {
        tech.Unlock();
        ApplyUnlockEffect(tech.Data.ID);

        if (currentResearch == tech)
            currentResearch = null;

        OnTechnologyUnlocked?.Invoke(tech);
        OnResearchStateChanged?.Invoke();
    }

    private void ApplyUnlockEffect(TechID id)
    {
        if (railBuilder == null)
            return;

        switch (id)
        {
            case TechID.LakeCrossingUnlock:
                railBuilder.AllowLakeCrossing(true);
                break;

            case TechID.MountainTunnelUnlock:
                railBuilder.AllowMountainTunnel(true);
                break;

            case TechID.SeaTunnelUnlock:
                railBuilder.AllowSeaTunnel(true);
                break;
        }
    }
}