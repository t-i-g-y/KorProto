using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainCardController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private GameObject leftArrowRoot;
    [SerializeField] private GameObject rightArrowRoot;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject speedRoot;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text maintenanceText;
    [SerializeField] private Button refundButton;
    [SerializeField] private TMP_Text refundText;
    [SerializeField] private TrainCardUI trainCard;
    private Train currentTrain;
    private int currentUnitIndex; 

    public Train CurrentTrain => currentTrain;
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(HideCard);

        if (leftButton != null)
            leftButton.onClick.AddListener(HandleGoLeft);

        if (rightButton != null)
            rightButton.onClick.AddListener(HandleGoRight);

        if (refundButton != null)
            refundButton.onClick.AddListener(HandleRefundClicked);
        
        HideCard();
    }

    private void Update()
    {
        if (IsOpen)
            Refresh();
    }

    public void ToggleTrain(Train train)
    {
        if (train == null)
            return;

        if (IsOpen && currentTrain == train)
        {
            HideCard();
            return;
        }

        ShowTrain(train);
    }

    public void ShowTrain(Train train)
    {
        bool sameTrain = currentTrain == train;
        currentTrain = train;

        if (!sameTrain)
            currentUnitIndex = -1;

        if (panelRoot != null)
            panelRoot.SetActive(train != null);

        Refresh();
    }

    public void HideCard()
    {
        currentTrain = null;
        currentUnitIndex = -1;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void HandleGoLeft()
    {
        if (currentTrain == null)
            return;

        if (currentUnitIndex > -1)
            currentUnitIndex--;

        Refresh();
    }

    private void HandleGoRight()
    {
        if (currentTrain == null || currentTrain.AttachedTrainConsist == null)
            return;

        int wagonCount = currentTrain.AttachedTrainConsist.WagonCount;
        if (currentUnitIndex < wagonCount - 1)
            currentUnitIndex++;

        Refresh();
    }

    private void HandleRefundClicked()
    {
        if (currentTrain == null || currentTrain.AttachedTrainConsist == null)
            return;

        TrainConsist consist = currentTrain.AttachedTrainConsist;
        bool isLocomotive = currentUnitIndex < 0;

        if (isLocomotive)
        {
            TrainManager.Instance.RemoveTrain(currentTrain);
            HideCard();
            return;
        }

        float refund = RailEconomySystem.Instance != null ? RailEconomySystem.Instance.CalculateWagonRefundCost() : 0f;

        if (consist.WagonCount > 0)
        {
            consist.RemoveLastWagon();
            currentTrain.SyncWagonViews();
            if (FinanceSystem.Instance != null && refund > 0f)
                FinanceSystem.Instance.ApplyRefund(refund);

            currentUnitIndex--;
            Refresh();
        }
    }
    private void Refresh()
    {
        if (currentTrain == null || currentTrain.AttachedTrainConsist == null)
            return;

        TrainConsist consist = currentTrain.AttachedTrainConsist;
        bool isLocomotive = currentUnitIndex < 0;
        int wagonCount = consist.WagonCount;

        if (titleText != null)
        {
            titleText.text = isLocomotive ? $"ЛОКОМОТИВ - Поезд#{currentTrain.ID}" : $"ВАГОН #{currentUnitIndex + 1}";
        }

        if (speedRoot != null)
        {
            speedRoot.gameObject.SetActive(isLocomotive);
            if (isLocomotive)
                speedText.text = GetSpeedLevelText(currentTrain.SpeedLevel);
        }

        int capacity = isLocomotive ? consist.GetHeadCapacity() : consist.GetUnitCapacity(currentUnitIndex);
        float maintenance = isLocomotive ? consist.GetHeadMaintenance() : consist.GetUnitMaintenance(currentUnitIndex);
        float refundPrice = isLocomotive ? RailEconomySystem.Instance.CalculateTrainRefundCost(currentTrain) : RailEconomySystem.Instance.CalculateWagonRefundCost();

        if (capacityText != null)
            capacityText.text = $"{capacity}";

        if (maintenanceText != null)
            maintenanceText.text = $"{maintenance:F2}";
        
        if (refundText != null)
            refundText.text = $"{refundPrice:F2}";

        int cargoStartIndex = isLocomotive ? 0 : consist.GetCargoStartIndexForWagon(currentUnitIndex);
        ResourceAmount[] slice = consist.BuildCargoSlice(cargoStartIndex, capacity);

        if (trainCard != null)
            trainCard.ShowCargo(slice, capacity);

        bool canGoLeft = !isLocomotive;
        bool canGoRight = isLocomotive ? wagonCount > 0 : currentUnitIndex < wagonCount - 1;

        if (leftArrowRoot != null)
            leftArrowRoot.SetActive(canGoLeft);

        if (rightArrowRoot != null)
            rightArrowRoot.SetActive(canGoRight);
    }

    private string GetSpeedLevelText(int speedLevel)
    {
        switch (speedLevel)
        {
            case 1:
                return "I";
            case 2:
                return "II";
            case 3:
                return "III";
            default:
                return speedLevel.ToString();
        }
    }
}
