using System.Text;
using TMPro;
using UnityEngine;

public class StationConsumedResourcesLabel : MonoBehaviour
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

        bool visible = station.IsSelected;
        textMesh.enabled = visible;

        if (!visible)
            return;

        textMesh.text = BuildConsumedResourcesText();
    }

    private string BuildConsumedResourcesText()
    {
        if (station.ConsumedResources == null || station.ConsumedResources.Count == 0)
            return "";

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("Необходимо:\n");

        for (int index = 0; index < station.ConsumedResources.Count; index++)
        {
            ResourceAmount consumedResource = station.ConsumedResources[index];
            if (index > 0)
                stringBuilder.AppendLine();

            stringBuilder.Append(consumedResource.Type);
            stringBuilder.Append(": ");
            stringBuilder.Append(station.GetDemandAmount(consumedResource.Type));
        }

        return stringBuilder.ToString();
    }
}
