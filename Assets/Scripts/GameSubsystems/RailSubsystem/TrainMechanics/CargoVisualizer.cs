using UnityEngine;
using System.Collections.Generic;

public class CargoVisualizer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] resourceSlots;
    [SerializeField] private Sprite[] resourceSprites;

    public int MaxDisplay => resourceSlots != null ? resourceSlots.Length : 0;

    public void VisualizeCargo(ResourceAmount[] resources)
    {
        if (resourceSlots == null)
            return;

        List<ResourceType> expanded = new();

        if (resources != null)
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                int index = (int)type;
                if (index < 0 || index >= resources.Length)
                    continue;

                int amount = Mathf.Max(0, resources[index].Amount);
                for (int i = 0; i < amount; i++)
                    expanded.Add(type);
            }
        }

        for (int i = 0; i < resourceSlots.Length; i++)
        {
            if (i < expanded.Count)
            {
                resourceSlots[i].enabled = true;
                resourceSlots[i].sprite = SpriteFor(expanded[i]);
            }
            else
            {
                resourceSlots[i].enabled = false;
                resourceSlots[i].sprite = null;
            }
        }
    }

    private Sprite SpriteFor(ResourceType resourceType)
    {
        int index = (int)resourceType;
        if (resourceSprites == null || index < 0 || index >= resourceSprites.Length)
            return null;

        return resourceSprites[index];
    }
}

