using UnityEngine;
using System.Collections.Generic;

public class CargoVisualizer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] slots;
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private Sprite triangleSprite;
    [SerializeField] private Sprite squareSprite;

    public int MaxDisplay => slots.Length;

    public void ShowCargo(ResourceAmount[] cargo)
    {
        List<ResourceType> list = new();

        int circles = cargo[(int)ResourceType.Circle].Amount;
        int triangles = cargo[(int)ResourceType.Triangle].Amount;
        int squares = cargo[(int)ResourceType.Square].Amount;

        for (int i = 0; i < circles; i++) 
            list.Add(ResourceType.Circle);
        for (int i = 0; i < triangles; i++) 
            list.Add(ResourceType.Triangle);
        for (int i = 0; i < squares; i++) 
            list.Add(ResourceType.Square);

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
            ResourceType.Circle => circleSprite,
            ResourceType.Triangle => triangleSprite,
            ResourceType.Square => squareSprite,
            _ => null
        };
    }
}

