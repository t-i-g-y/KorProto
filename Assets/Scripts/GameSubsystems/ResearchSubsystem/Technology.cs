using System;

[Serializable]
public class Technology
{
    public TechData Data { get; private set; }
    public float Progress { get; private set; }
    public bool IsUnlocked { get; private set; }
    public bool IsResearching { get; private set; }

    public Technology(TechData data)
    {
        Data = data;
        Progress = 0f;
        IsUnlocked = false;
        IsResearching = false;
    }

    public bool IsComplete => Progress >= Data.researchCost;

    public float ProgressNormalized
    {
        get
        {
            if (Data == null || Data.researchCost <= 0)
                return 0f;

            return (float)Progress / Data.researchCost;
        }
    }

    public void StartResearch()
    {
        if (!IsUnlocked)
            IsResearching = true;
    }

    public void StopResearch()
    {
        IsResearching = false;
    }

    public void AddProgress(int amount)
    {
        if (amount <= 0 || IsUnlocked || Data == null)
            return;

        Progress += amount;

        if (Progress > Data.researchCost)
            Progress = Data.researchCost;
    }

    public void Unlock()
    {
        IsUnlocked = true;
        IsResearching = false;

        if (Data != null)
            Progress = Data.researchCost;
    }

    #region save subsystem
    public void LoadFromSaveData(float savedProgress, bool savedUnlocked, bool savedResearching)
    {
        Progress = savedProgress < 0f ? 0f : savedProgress;
        IsUnlocked = savedUnlocked;
        IsResearching = savedResearching;

        if (Data != null && Progress > Data.researchCost)
            Progress = Data.researchCost;

        if (IsUnlocked && Data != null)
            Progress = Data.researchCost;
    }
    #endregion
}