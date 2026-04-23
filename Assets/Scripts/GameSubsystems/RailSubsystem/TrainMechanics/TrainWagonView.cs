using UnityEngine;

public class TrainWagonView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CargoVisualizer cargoVisualizer;
    [SerializeField] private float wagonSpacing = 0.9f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color brokenColor = Color.darkGray;

    private Train assignedTrain;
    private int wagonIndex;

    public CargoVisualizer CargoVisualizer => cargoVisualizer;

    public void Initialize(Train train, int wagonIndex)
    {
        assignedTrain = train;
        this.wagonIndex = wagonIndex;
        HandleFollowMovement(true);
    }

    private void LateUpdate()
    {
        HandleFollowMovement(false);
    }

    public void HandleFollowMovement(bool snap)
    {
        if (assignedTrain == null)
            return;

        assignedTrain.GetPoseAtDistanceBehindHead(wagonSpacing * (wagonIndex + 1), out Vector3 position,out Vector3 forward);

        if (snap)
            transform.position = position;
        else
            transform.position = position;

        if (forward.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(Vector3.forward, forward);
    }

    public void SetSelectedVisual(bool isSelected)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = isSelected ? selectedColor : (assignedTrain.IsBroken ? brokenColor : normalColor);
    }
}
