using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITrainLineEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text trainText;
    [SerializeField] private TMP_Text lineText;
    [SerializeField] private Button speedButton;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private Button capacityButton;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text selectText;
    [SerializeField] private Image selectImage;
    [SerializeField] private Button deleteButton;

    [SerializeField] private Color32 selectColor = new Color32(0, 204, 68, 255);
    [SerializeField] private Color32 selectTextColor = new Color32(50, 50, 50, 255);
    [SerializeField] private Color32 deselectColor = new Color32(0, 25, 75, 255);
    [SerializeField] private Color32 deselectTextColor = new Color32(248, 248, 248, 255);

    public Train ReferenceTrain { get; private set; }
    public RailLine ReferenceLine { get; private set; }
    public bool IsSelected { get; private set; }
    public event Action<UITrainLineEntry> OnSelectClicked;
    public event Action<UITrainLineEntry> OnDeleteClicked;

    public void Init(Train train, RailLine line)
    {
        ReferenceTrain = train;
        ReferenceLine = line;
        
        if (lineText != null)
            lineText.text = $"Line {line.ID}";

        selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke(this));
        deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(this));
    }
}
