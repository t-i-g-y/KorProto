using TMPro;
using UnityEngine;

public class StationPopulationLabel : MonoBehaviour
{
    private Station station;
    private TextMeshPro textMesh;

    private void Awake()
    {
        station = GetComponentInParent<Station>();
        textMesh = GetComponent<TextMeshPro>();

        if (textMesh != null)
            textMesh.enabled = false;
    }

    private void LateUpdate()
    {
        if (station == null || textMesh == null)
            return;

        textMesh.text = station.Population.ToString();
        textMesh.enabled = station.IsSelected;
    }
}
