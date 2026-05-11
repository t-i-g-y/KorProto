using System;
using System.Collections.Generic;
using UnityEngine;

public class ResearchSystem : MonoBehaviour
{
    public static ResearchSystem Instance { get; private set; }
    [SerializeField] private List<TechData> techDatabase = new();
    private readonly Dictionary<TechID, Technology> technologies = new();
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
        DontDestroyOnLoad(gameObject);
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
        {
            Debug.Log($"CanResearch fail: {id} tech missing");
            return false;
        }

        if (tech.IsUnlocked)
            return false;

        foreach (TechID prereqId in tech.Data.prerequisites)
        {
            if (!IsUnlocked(prereqId))
            {
                Debug.Log($"{id} blocked by prereq {prereqId}");
                return false;
            }
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

    private void ApplyUnlockEffect(TechID ID)
    {
        if (ResearchModifierSystem.Instance == null)
            return;

        ResearchModifierSystem.Instance.ApplyTechnology(ID);
    }

    private void ReapplyUnlockedEffects()
    {
        foreach (var pair in technologies)
        {
            Technology tech = pair.Value;

            if (tech != null && tech.IsUnlocked && tech.Data != null)
                ApplyUnlockEffect(tech.Data.ID);
        }
    }



    #region save subsystem
    public ResearchSystemSaveData GetSaveData()
    {
        var data = new ResearchSystemSaveData();

        if (currentResearch != null && currentResearch.Data != null)
            data.currentResearchID = (int)currentResearch.Data.ID;
        else
            data.currentResearchID = -1;

        foreach (var technology in technologies)
        {
            Technology tech = technology.Value;
            if (tech == null || tech.Data == null)
                continue;

            data.technologies.Add(new TechnologySaveData
            {
                techID = (int)tech.Data.ID,
                progress = tech.Progress,
                isUnlocked = tech.IsUnlocked,
                isResearching = tech.IsResearching
            });
        }

        return data;
    }

    public void LoadFromSaveData(ResearchSystemSaveData data)
    {
        if (data == null)
            return;

        BuildRuntimeTechnologies();

        currentResearch = null;

        foreach (var savedTech in data.technologies)
        {
            TechID ID = (TechID)savedTech.techID;

            Technology tech = GetTechnology(ID);
            if (tech == null)
                continue;

            tech.LoadFromSaveData(savedTech.progress, savedTech.isUnlocked, savedTech.isResearching);
        }

        if (data.currentResearchID != -1)
        {
            TechID currentID = (TechID)data.currentResearchID;
            Technology loadedCurrentResearch = GetTechnology(currentID);

            if (loadedCurrentResearch != null && !loadedCurrentResearch.IsUnlocked)
            {
                currentResearch = loadedCurrentResearch;
                currentResearch.StartResearch();
                OnResearchStarted?.Invoke(currentResearch);
            }
        }

        ReapplyUnlockedEffects();
        OnResearchStateChanged?.Invoke();
    }
    #endregion
}