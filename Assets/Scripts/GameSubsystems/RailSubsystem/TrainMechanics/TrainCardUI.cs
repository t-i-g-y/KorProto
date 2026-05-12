using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainCardUI : MonoBehaviour
{
    [SerializeField] private Image[] slots;
    [SerializeField] private Sprite[] resourceSprites;
    [SerializeField] private Color[] resourceColors;
    [SerializeField] private TMP_Text[] resourceTexts;
    [SerializeField] private Sprite emptySlotSprite;
    [SerializeField] private Color filledColor;
    [SerializeField] private Color emptyColor;
    [SerializeField] private Color hiddenColor;

    public void ShowCargo(ResourceAmount[] resources, int capacity)
    {
        if (slots == null)
            return;

        int filledCount = CountFilled(resources);
        ResourceType[] expanded = UpdateCapacity(resources, filledCount);

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            bool insideCapacity = i < capacity;
            slots[i].gameObject.SetActive(insideCapacity);

            if (!insideCapacity)
                continue;

            if (i < expanded.Length)
            {
                slots[i].sprite = resourceSprites[(int)expanded[i]];
                slots[i].color = resourceColors[(int)expanded[i]];
                resourceTexts[i].text = GetResourceText(expanded[i]);
            }
            else
            {
                slots[i].sprite = emptySlotSprite;
                slots[i].color = emptyColor;
                resourceTexts[i].text = "";
            }
        }
    }

    private int CountFilled(ResourceAmount[] resources)
    {
        if (resources == null)
            return 0;

        int total = 0;

        for (int i = 0; i < resources.Length; i++)
            total += Mathf.Max(0, resources[i].Amount);

        return total;
    }

    private ResourceType[] UpdateCapacity(ResourceAmount[] resources, int totalCount)
    {
        if (resources == null || totalCount <= 0)
            return new ResourceType[0];

        ResourceType[] cargo = new ResourceType[totalCount];
        int writeIndex = 0;

        foreach (ResourceType resource in System.Enum.GetValues(typeof(ResourceType)))
        {
            int index = (int)resource;
            if (index < 0 || index >= resources.Length)
                continue;

            int amount = Mathf.Max(0, resources[index].Amount);

            for (int i = 0; i < amount; i++)
            {
                if (writeIndex >= cargo.Length)
                    break;

                cargo[writeIndex] = resource;
                writeIndex++;
            }
        }

        return cargo;
    }

    private string GetResourceText(ResourceType resource)
    {
        switch(resource)
        {
            case ResourceType.Coal:
                return "Уголь";
            case ResourceType.Iron:
                return "Жлз.";
            case ResourceType.Milk:
                return "Продукты";
            case ResourceType.Water:
                return "Вода";
            case ResourceType.Millet:
                return "Пшен.";
            default:
                return resource.ToString();

        }
            
    }


}
