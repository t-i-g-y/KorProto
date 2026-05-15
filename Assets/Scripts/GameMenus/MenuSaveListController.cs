using UnityEngine;

public class MenuSaveListController : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private MenuSaveSlot slotPrefab;
    [SerializeField] private MainMenu mainMenu;

    private SubsystemSaveManager saveManager;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (saveManager == null)
            saveManager = FindFirstObjectByType<SubsystemSaveManager>();

        if (saveManager == null)
        {
            Debug.Log("SubsystemSaveManager not found");
            return;
        }

        if (slotPrefab == null || mainMenu == null)
        {
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        foreach (var saveInfo in saveManager.GetSaveInfos())
        {
            MenuSaveSlot slot = Instantiate(slotPrefab, contentRoot);
            slot.Setup(this, saveInfo);
        }
    }

    public void LoadSave(string saveName)
    {
        mainMenu.LoadSpecificSave(saveName);
    }

    public void DeleteSave(string saveName)
    {
        saveManager.DeleteSave(saveName);
        Refresh();
    }

    
}
