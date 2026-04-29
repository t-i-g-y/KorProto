using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class ArtifactPickup : MonoBehaviour
{
    private ArtifactManager manager;
    private string artifactId;
    private Vector3Int cell;

    public string ArtifactId => artifactId;
    public Vector3Int Cell => cell;

    public void Initialize(ArtifactManager artifactManager, ArtifactDefinition definition, Vector3Int spawnCell, Sprite sprite)
    {
        manager = artifactManager;
        artifactId = definition != null ? definition.ArtifactId : string.Empty;
        cell = spawnCell;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 30;

        BoxCollider2D artifactCollider = GetComponent<BoxCollider2D>();
        artifactCollider.isTrigger = false;

        if (sprite != null)
        {
            artifactCollider.offset = sprite.bounds.center;
            artifactCollider.size = sprite.bounds.size;
        }
        else
        {
            artifactCollider.offset = Vector2.zero;
            artifactCollider.size = Vector2.one;
        }
    }

    private void OnMouseDown()
    {
        manager?.Collect(this);
    }
}
