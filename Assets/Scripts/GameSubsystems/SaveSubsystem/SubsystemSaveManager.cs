using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SubsystemSaveManager : MonoBehaviour
{
    private string folderPath => Path.Combine(Application.persistentDataPath, "saves");
    private static string activeSaveName;
    public void SaveGame(string saveName = null)
    {
        if (!string.IsNullOrWhiteSpace(saveName))
            activeSaveName = saveName;

        if (string.IsNullOrWhiteSpace(activeSaveName))
            activeSaveName = GenerateNewSaveName();

        SubsystemSaveData data = new SubsystemSaveData
        {
            anchorData = RailAnchorRegistry.Instance.GetSaveData(),
            railData = RailManager.Instance.GetSaveData(),
            relayStopData = RelayStopRegistry.Instance.GetSaveData(),
            globalDemandData = GlobalDemandSystem.Instance.GetSaveData(),
            timeData = TimeManager.Instance.GetSaveData(),
            financeData = FinanceSystem.Instance.GetSaveData(),
            trainData = TrainManager.Instance.GetSaveData(),
            economyData = EconomyManager.Instance.GetSaveData(),
            researchData = ResearchSystem.Instance.GetSaveData()
        };

        string json = JsonUtility.ToJson(data, true);
        string path = GetPath(activeSaveName);

        File.WriteAllText(path, json);

        Debug.Log("game saved " + path);
    }

    public void LoadGame(string saveName = "save_001")
    {
        string path = GetPath(saveName);

        if (!File.Exists(path))
        {
            Debug.LogWarning("no save file: " + path);
            return;
        }

        activeSaveName = saveName;

        string json = File.ReadAllText(path);
        SubsystemSaveData data = JsonUtility.FromJson<SubsystemSaveData>(json);

        if (data == null)
        {
            Debug.LogError("failed to read save file");
            return;
        }

        if (data.anchorData != null)
            RailAnchorRegistry.Instance.LoadFromSaveData(data.anchorData);

        if (data.railData != null)
            RailManager.Instance.LoadFromSaveData(data.railData);

        if (data.relayStopData != null)
            RelayStopRegistry.Instance.LoadFromSaveData(data.relayStopData);

        if (data.globalDemandData != null)
            GlobalDemandSystem.Instance.LoadFromSaveData(data.globalDemandData);

        if (data.timeData != null)
            TimeManager.Instance.LoadFromSaveData(data.timeData);

        if (data.financeData != null)
            FinanceSystem.Instance.LoadFromSaveData(data.financeData);

        if (data.trainData != null)
            TrainManager.Instance.LoadFromSaveData(data.trainData);

        if (data.economyData != null)
            EconomyManager.Instance.LoadFromSaveData(data.economyData);

        if (data.researchData != null)
            ResearchSystem.Instance.LoadFromSaveData(data.researchData);

        Debug.Log("game loaded " + path);
    }
    public string GenerateNewSaveName()
    {
        int index = 1;

        while (SaveExists($"save_{index:000}"))
            index++;

        return $"save_{index:000}";
    }

    private string GetPath(string saveName = "save")
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return Path.Combine(folderPath, saveName + ".json");
    }

    public bool SaveExists(string saveName = "save")
    {
        return File.Exists(GetPath(saveName));
    }

    public List<string> GetSaveNames()
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        List<string> saves = new List<string>();

        foreach (string file in Directory.GetFiles(folderPath, "*.json"))
            saves.Add(Path.GetFileNameWithoutExtension(file));

        return saves;
    }

    public List<SaveSlotInfo> GetSaveInfos()
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        
        List<SaveSlotInfo> result = new();

        foreach (string file in Directory.GetFiles(folderPath, "*.json"))
        {
            result.Add(new SaveSlotInfo
            {
                SaveName = Path.GetFileNameWithoutExtension(file),
                SaveTime = File.GetLastWriteTime(file)
            });
        }

        result.Sort((a, b) => b.SaveTime.CompareTo(a.SaveTime));

        return result;
    }

    public string GetLatestSaveName()
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
            
        string[] files = Directory.GetFiles(folderPath, "*.json");

        if (files.Length == 0)
            return null;

        string latestFile = files[0];

        foreach (string file in files)
        {
            if (File.GetLastWriteTime(file) > File.GetLastWriteTime(latestFile))
                latestFile = file;
        }

        return Path.GetFileNameWithoutExtension(latestFile);
    }
    public void DeleteSave(string saveName)
    {
        string path = GetPath(saveName);

        if (File.Exists(path))
            File.Delete(path);
    }
}
