using UnityEngine;
using UnityEngine.UI;

public class UIRailListController : MonoBehaviour
{
    [SerializeField] private Button viewListButton;
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject listEntryPrefab;

    void Awake()
    {
        listPanel.SetActive(false);
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }
}
