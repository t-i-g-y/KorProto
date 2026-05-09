using System.Text;
using TMPro;
using UnityEngine;

public class StationInfoLabel : MonoBehaviour
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
