using System.Text;
using TMPro;
using UnityEngine;

public class StationProducedResourcesLabel : MonoBehaviour
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
        if (station == null || textMesh == null || cam == null)
            return;

        bool visible = cam.orthographicSize <= showAtZoom;
        textMesh.enabled = visible;

        if (!visible)
            return;

        textMesh.text = BuildProducedResourcesText();
    }

    private string BuildProducedResourcesText()
    {
        if (station.ProducedResources == null || station.ProducedResources.Count == 0)
            return string.Empty;

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Производство:\n");

        for (int index = 0; index < station.ProducedResources.Count; index++)
        {
            ResourceAmount producedResource = station.ProducedResources[index];
            if (index > 0)
                stringBuilder.AppendLine();

            stringBuilder.Append(producedResource.Type);
            stringBuilder.Append(": ");
            stringBuilder.Append(station.GetSupplyAmount(producedResource.Type));
        }

        return stringBuilder.ToString();
    }
}