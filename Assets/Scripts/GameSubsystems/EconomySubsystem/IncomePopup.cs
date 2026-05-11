using TMPro;
using UnityEngine;

public class IncomePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float riseSpeed = 0.65f;
    [SerializeField] private Vector3 riseDirection = new Vector3(0f, 1f, 0f);

    private Transform anchor;
    private Vector3 anchorOffset;
    private Color baseColor;
    private float timer;

    public void Initialize(string text, Transform followAnchor)
    {
        anchor = followAnchor;

        if (anchor != null)
            anchorOffset = transform.position - anchor.position;

        if (textMesh != null)
        {
            textMesh.text = text;
            baseColor = textMesh.color;
        }
    }

    private void Awake()
    {
        if (textMesh != null)
            baseColor = textMesh.color;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        Vector3 basePosition = transform.position;

        if (anchor != null)
            basePosition = anchor.position + anchorOffset;

        float rise = riseSpeed * timer;
        transform.position = basePosition + riseDirection.normalized * rise;

        float t = Mathf.Clamp01(timer / lifetime);
        float alpha = 1f - t;

        if (textMesh != null)
        {
            Color c = baseColor;
            c.a = alpha;
            textMesh.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}