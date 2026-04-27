using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private SubsystemSaveManager saveManager;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private bool wasTimePausedBeforeMenu;
    public bool IsOpen => rootPanel != null && rootPanel.activeSelf;

    private void Awake()
    {
        CloseMenuImmediate();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            if (IsOpen)
                ResumeGame();
            else
                OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (IsOpen)
            return;

        MenuPauseState.SetPaused(true);

        if (TimeManager.Instance != null)
        {
            wasTimePausedBeforeMenu = TimeManager.Instance.TimeMultiplier <= 0f;
            TimeManager.Instance.Pause();
        }
        else
        {
            wasTimePausedBeforeMenu = false;
        }

        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        if (!IsOpen)
            return;

        if (rootPanel != null)
            rootPanel.SetActive(false);

        MenuPauseState.SetPaused(false);

        if (TimeManager.Instance != null && !wasTimePausedBeforeMenu)
            TimeManager.Instance.Unpause();
    }

    public void SaveGame()
    {
        if (saveManager == null)
            saveManager = FindFirstObjectByType<SubsystemSaveManager>();

        if (saveManager == null)
        {
            Debug.Log("[GameMenuController] SubsystemSaveManager not found");
            return;
        }

        saveManager.SaveGame();
    }

    public void OpenSettings()
    {
        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        MenuPauseState.SetPaused(false);

        if (rootPanel != null)
            rootPanel.SetActive(false);

        DestroyIfExists<TimeManager>();
        DestroyIfExists<EconomyManager>();
        DestroyIfExists<FinanceSystem>();
        DestroyIfExists<GlobalDemandSystem>();
        DestroyIfExists<RailEconomySystem>();
        DestroyIfExists<StationEconomySystem>();
        DestroyIfExists<IncomePopupSpawner>();
        DestroyIfExists<TrainManager>();
        DestroyIfExists<RailManager>();
        DestroyIfExists<RailAnchorRegistry>();
        DestroyIfExists<RelayStopRegistry>();
        DestroyIfExists<ResearchSystem>();
        DestroyIfExists<ResearchIncomeSystem>();
        DestroyIfExists<ResearchModifierSystem>();
        DestroyIfExists<HexRailNetwork>();

        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void CloseMenuImmediate()
    {
        MenuPauseState.SetPaused(false);

        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (mainPanel != null)
            mainPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void DestroyIfExists<T>() where T : MonoBehaviour
    {
        T obj = FindFirstObjectByType<T>();

        if (obj != null)
            Destroy(obj.gameObject);
    }
}
