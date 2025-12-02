using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIRailLineEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text lineName;
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text selectText;
    [SerializeField] private Image selectImage;
    [SerializeField] private Button deleteButton;

    [SerializeField] private Color32 selectColor = new Color32(0, 204, 68, 255);
    [SerializeField] private Color32 selectTextColor = new Color32(50, 50, 50, 255);
    [SerializeField] private Color32 deselectColor = new Color32(0, 25, 75, 255);
    [SerializeField] private Color32 deselectTextColor = new Color32(248, 248, 248, 255);

    public RailLine ReferenceLine { get; private set; }
    public bool IsSelected { get; private set; }
    public event Action<UIRailLineEntry> OnSelectClicked;
    public event Action<UIRailLineEntry> OnDeleteClicked;

    public void Init(RailLine line)
    {
        ReferenceLine = line;
        
        if (lineName != null)
            lineName.text = $"Line {line.ID}";

        SetSelected(false);

        selectButton.onClick.AddListener(() => OnSelectClicked?.Invoke(this));
        deleteButton.onClick.AddListener(() => OnDeleteClicked?.Invoke(this));
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (selectText != null)
        {
            selectText.text = selected ? "DESELECT" : "SELECT";
            selectText.color = selected ? deselectTextColor : selectTextColor;
        }

        if (selectImage != null)
        {
            selectImage.color = selected ? deselectColor : selectColor;
        }
    }
}
