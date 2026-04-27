using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private string path => Application.persistentDataPath + "/save.json";

    public void SaveGame()
    {
        GameSaveData data = new GameSaveData
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
        File.WriteAllText(path, json);

        Debug.Log("game saved " + path);
    }

    public void LoadGame()
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file");
            return;
        }

        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

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

        Debug.Log("game loaded");
    }
}
