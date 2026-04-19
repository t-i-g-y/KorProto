using UnityEngine;

public class RailAnchor : MonoBehaviour
{
    [SerializeField] private Vector3Int cell;
    [SerializeField] private bool isCore = true;

    public Vector3Int Cell => cell;
    public bool IsCore => isCore;

#if UNITY_EDITOR
    private void OnValidate()
    {
        cell = Vector3Int.RoundToInt(transform.position);
    }
#endif

    #region save subsystem
    public RailAnchorSaveData GetSaveData()
    {
        return new RailAnchorSaveData
        {
            cell = cell,
            isCore = isCore
        };
    }

    public void LoadFromSaveData(RailAnchorSaveData data)
    {
        if (data == null)
            return;

        cell = data.cell;
        isCore = data.isCore;

        transform.position = cell;
    }
    #endregion

}
