using TMPro;
using UnityEngine;

public class StationPopulationLabel : MonoBehaviour
{
    [SerializeField] private float showAtZoom = 3.5f;

    private Station station;
    private TextMeshPro textMesh;
    private Camera cam;

    private void Awake()
    {
        station = GetComponentInParent<Station>();
        textMesh = GetComponent<TextMeshPro>();
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (station == null) return;

        textMesh.text = station.Population.ToString();
        textMesh.enabled = cam.orthographicSize <= showAtZoom;
    }
}
