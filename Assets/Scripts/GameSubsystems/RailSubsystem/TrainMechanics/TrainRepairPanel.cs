using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrainRepairPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button repairButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

    private RectTransform panelRect;
    private Transform target;
    private Train currentTrain;
    public Train CurrentTrain => currentTrain;

    private void Awake()
    {
        panelRect = panelRoot != null ? panelRoot.GetComponent<RectTransform>() : GetComponent<RectTransform>();

        if (repairButton != null)
            repairButton.onClick.AddListener(HandleRepair);

        if (sellButton != null)
            sellButton.onClick.AddListener(HandleSell);

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        Hide();
    }

    private void LateUpdate()
    {
        if (target == null || panelRect == null || Camera.main == null || panelRoot == null || !panelRoot.activeSelf)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
        panelRect.position = screenPos;
    }

    public void Show(Train train)
    {
        currentTrain = train;
        target = train != null ? train.transform : null;

        if (panelRoot != null)
            panelRoot.SetActive(train != null);

        if (train == null)
            return;

        if (titleText != null)
            titleText.text = $"Поезд#{train.ID} сломался!";

        if (costText != null)
            costText.text = $"Починить: {train.RepairCost:0} | Продать: {50}";
    }

    public void Hide()
    {
        Train trainToDeselect = currentTrain;

        currentTrain = null;
        target = null;

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (trainToDeselect != null && TrainManager.Instance != null)
            TrainManager.Instance.ForceDeselect(trainToDeselect);
    }

    private void HandleRepair()
    {
        if (currentTrain == null || FinanceSystem.Instance == null)
            return;

        float cost = currentTrain.RepairCost;
        if (FinanceSystem.Instance.Balance < cost)
            return;

        Train trainToRepair = currentTrain;

        FinanceSystem.Instance.AdjustBalance(-cost);
        trainToRepair.Repair();

        Hide();
    }

    private void HandleSell()
    {
        if (currentTrain == null)
            return;

        Train trainToSell = currentTrain;

        if (FinanceSystem.Instance != null)
            FinanceSystem.Instance.AdjustBalance(50);

        Hide();
        TrainManager.Instance.RemoveTrain(trainToSell);
    }
}