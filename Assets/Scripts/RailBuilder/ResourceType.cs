using System;
using UnityEngine;

public enum ResourceType
{
    Coal,
    Iron,
    Milk,
    Water,
    Millet,
    Plastic,
    Circle
}

[Serializable]
public struct ResourceAmount
{
    [SerializeField] private ResourceType type;
    [SerializeField] private int amount;

    public ResourceAmount(ResourceType type)
    {
        this.type = type;
        amount = 0;
    }

    public ResourceAmount(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }

    public ResourceType Type
    {
        get => type;
        set => type = value;
    }

    public int Amount
    {
        get => amount;
        set => amount = value;
    }

    public static ResourceAmount operator ++(ResourceAmount resourceAmount) =>
        new(resourceAmount.Type, resourceAmount.Amount + 1);

    public static ResourceAmount operator --(ResourceAmount resourceAmount) =>
        new(resourceAmount.Type, resourceAmount.Amount - 1);

    public static ResourceAmount operator +(ResourceAmount resourceAmount, int value) =>
        new(resourceAmount.Type, resourceAmount.Amount + value);

    public static ResourceAmount operator -(ResourceAmount resourceAmount, int value) =>
        new(resourceAmount.Type, resourceAmount.Amount - value);
}