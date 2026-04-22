using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrokenTrainNotifierUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private TrainRepairPanel repairPanel;

    private int currentIndex = 0;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);

        Refresh();
    }

    private void OnEnable()
    {
        Train.TrainBroken += HandleTrainStateChanged;
        Train.TrainRepaired += HandleTrainStateChanged;
        TrainManager.TrainCreated += HandleTrainCreated;
        TrainManager.TrainRemoved += HandleTrainRemoved;
        Refresh();
    }

    private void OnDisable()
    {
        Train.TrainBroken -= HandleTrainStateChanged;
        Train.TrainRepaired -= HandleTrainStateChanged;
        TrainManager.TrainCreated -= HandleTrainCreated;
        TrainManager.TrainRemoved -= HandleTrainRemoved;
    }

    private void HandleTrainStateChanged(Train train)
    {
        Refresh();
    }

    private void HandleTrainCreated(Train train, RailLine line)
    {
        Refresh();
    }

    private void HandleTrainRemoved(Train train)
    {
        Refresh();
    }

    private void Refresh()
    {
        List<Train> broken = TrainManager.Instance != null ? TrainManager.Instance.GetBrokenTrains() : null;
        int count = broken != null ? broken.Count : 0;

        if (root != null)
            root.SetActive(count > 0);

        if (countText != null)
            countText.text = count.ToString();

        if (count == 0)
            currentIndex = 0;
        else
            currentIndex = Mathf.Clamp(currentIndex, 0, count - 1);
    }

    private void HandleClick()
    {
        if (TrainManager.Instance == null)
            return;

        List<Train> broken = TrainManager.Instance.GetBrokenTrains();
        if (broken.Count == 0)
        {
            Refresh();
            return;
        }

        if (currentIndex >= broken.Count)
            currentIndex = 0;

        Train train = broken[currentIndex];
        currentIndex = (currentIndex + 1) % broken.Count;

        TrainManager.Instance.PanToTrain(train);

        if (TrainManager.Instance.SelectedTrain != train)
            TrainManager.Instance.ToggleSelection(train);

        if (repairPanel != null)
            repairPanel.Show(train);

        Refresh();
    }
}
