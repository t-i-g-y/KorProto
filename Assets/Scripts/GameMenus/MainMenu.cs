using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "KORGameSubsystemsSandbox";
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject loadPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button continueButton;

    private SubsystemSaveManager saveManager;

    private void Awake()
    {
        saveManager = FindFirstObjectByType<SubsystemSaveManager>();

        if (saveManager == null)
        {
            Debug.Log("[Mainmenu] SubsystemSaveManager not found");
            return;
        }

        if (continueButton != null)
            continueButton.interactable = saveManager.GetLatestSaveName() != null;

        BackToMainPanel();
    }

    public void NewGame()
    {
        StartCoroutine(LoadSceneAndCreateNewSave());
    }

    public void ContinueGame()
    {
        StartCoroutine(LoadSceneAndContinue());
    }

    public void OpenLoadMenu()
    {
        mainPanel.SetActive(false);
        loadPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        loadPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToMainPanel()
    {
        mainPanel.SetActive(true);
        loadPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator LoadSceneAndCreateNewSave()
    {
        DontDestroyOnLoad(gameObject);

        string saveName = saveManager.GenerateNewSaveName();

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        while (!operation.isDone)
            yield return null;

        yield return null;

        SubsystemSaveManager gameSaveManager = FindFirstObjectByType<SubsystemSaveManager>();

        if (gameSaveManager != null)
            gameSaveManager.SaveGame(saveName);
        else
            Debug.Log("[Mainmenu] SubsystemSaveManager not found");

        Destroy(gameObject);
    }

    private IEnumerator LoadSceneAndLoadSave(string saveName)
    {
        DontDestroyOnLoad(gameObject);

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        while (!operation.isDone)
            yield return null;

        yield return null;

        SubsystemSaveManager saveManager = FindFirstObjectByType<SubsystemSaveManager>();

        if (saveManager != null)
            saveManager.LoadGame(saveName);
        else
            Debug.Log("[Mainmenu] SubsystemSaveManager not found");

        Destroy(gameObject);
    }

    private IEnumerator LoadSceneAndContinue()
    {
        DontDestroyOnLoad(gameObject);

        string saveName = saveManager.GetLatestSaveName();

        if (string.IsNullOrEmpty(saveName))
        {
            Debug.Log("No saves found.");
            Destroy(gameObject);
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(gameSceneName);
        while (!operation.isDone)
            yield return null;

        yield return null;

        SubsystemSaveManager gameSaveManager = FindFirstObjectByType<SubsystemSaveManager>();

        if (gameSaveManager != null)
            gameSaveManager.LoadGame(saveName);
        else
            Debug.Log("[Mainmenu] SubsystemSaveManager not found");

        Destroy(gameObject);
    }

    public void LoadSpecificSave(string saveName)
    {
        StartCoroutine(LoadSceneAndLoadSave(saveName));
    }
}
