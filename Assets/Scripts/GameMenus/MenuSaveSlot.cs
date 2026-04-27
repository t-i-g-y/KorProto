using TMPro;
using UnityEngine;
using System;

public class MenuSaveSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text saveNameText;
    [SerializeField] private TMP_Text saveTimeText;
    private MenuSaveListController saveList;
    private SaveSlotInfo slotInfo;

    public void Setup(MenuSaveListController saveList, SaveSlotInfo slotInfo)
    {
        this.slotInfo = slotInfo;
        this.saveList = saveList;

        if (saveNameText != null)
            saveNameText.text = slotInfo.SaveName;
        
        if (saveTimeText != null)
            saveTimeText.text = slotInfo.SaveTime.ToString("dd.MM.yyyy HH:mm:ss");
    }

    public void Load()
    {
        saveList.LoadSave(slotInfo.SaveName);
    }

    public void Delete()
    {
        saveList.DeleteSave(slotInfo.SaveName);
    }
}

public struct SaveSlotInfo
{
    public string SaveName;
    public DateTime SaveTime;
}
