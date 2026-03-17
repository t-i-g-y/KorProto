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
}
