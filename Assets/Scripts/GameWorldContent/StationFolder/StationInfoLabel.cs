using TMPro;
using UnityEngine;

public class StationInfoLabel : MonoBehaviour
{
    private TextMeshPro textMesh;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();

        if (textMesh != null)
            textMesh.enabled = false;
    }

    private void LateUpdate()
    {
        if (textMesh != null)
            textMesh.enabled = false;
    }
}
