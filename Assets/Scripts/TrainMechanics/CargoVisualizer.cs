using UnityEngine;
using System.Collections.Generic;

public class CargoVisualizer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] slots;
    [SerializeField] private Sprite coalSprite;
    [SerializeField] private Sprite ironSprite;
    [SerializeField] private Sprite milkSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite milletSprite;
    [SerializeField] private Sprite plasticSprite;

    public int MaxDisplay => slots.Length;

    public void ShowCargo(ResourceAmount[] cargo)
    {
        List<ResourceType> list = new();

        foreach (ResourceType resourceType in System.Enum.GetValues(typeof(ResourceType)))
        {
            int amount = cargo[(int)resourceType].Amount;
            for (int index = 0; index < amount; index++)
                list.Add(resourceType);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < list.Count)
            {
                slots[i].enabled = true;
                slots[i].sprite = SpriteFor(list[i]);
            }
            else
            {
                slots[i].enabled = false;
            }
        }
    }

    private Sprite SpriteFor(ResourceType resource)
    {
        return resource switch
        {
            ResourceType.Coal => coalSprite,
            ResourceType.Iron => ironSprite,
            ResourceType.Milk => milkSprite,
            ResourceType.Water => waterSprite,
            ResourceType.Millet => milletSprite,
            ResourceType.Plastic => plasticSprite,
            _ => null
        };
    }
}

