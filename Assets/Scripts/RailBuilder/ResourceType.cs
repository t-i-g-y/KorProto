using System;
using UnityEngine;

public enum ResourceType
{
    Circle,
    Triangle,
    Square
}

[Serializable]
public struct ResourceAmount
{
    [SerializeField] private ResourceType _type;
    [SerializeField]private int _amount;

    public ResourceAmount(ResourceType type)
    {
        _type = type;
        _amount = 0;
    }
    public ResourceAmount(ResourceType type, int amount)
    {
        _type = type;
        _amount = amount;
    }

    public ResourceType Type
    {
        get => _type;
        set => _type = value;
    }

    public int Amount
    {
        get => _amount;
        set => _amount = value;
    }

    public static ResourceAmount operator ++(ResourceAmount resourceAmount) => new ResourceAmount(resourceAmount.Type, resourceAmount.Amount + 1);

    public static ResourceAmount operator --(ResourceAmount resourceAmount) => new ResourceAmount(resourceAmount.Type, resourceAmount.Amount - 1);

    public static ResourceAmount operator +(ResourceAmount resourceAmount, int n) => new ResourceAmount(resourceAmount.Type, resourceAmount.Amount + n);

    public static ResourceAmount operator -(ResourceAmount resourceAmount, int n) => new ResourceAmount(resourceAmount.Type, resourceAmount.Amount - n);

    
}