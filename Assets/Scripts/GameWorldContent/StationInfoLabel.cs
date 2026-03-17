using System.Text;
using TMPro;
using UnityEngine;

public class StationInfoLabel : MonoBehaviour
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

        bool visible = cam.orthographicSize <= showAtZoom;
        textMesh.enabled = visible;

        if (!visible) return;

        var sb = new StringBuilder();

        if (station.Attributes.Count > 0)
        {
            for (int i = 0; i < station.Attributes.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(station.Attributes[i].AttributeType);
            }
        }

        textMesh.text = sb.ToString();
    }
}
