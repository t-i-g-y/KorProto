using System;

[Serializable]
public struct CargoSaleResult
{
    public bool Sold;
    public ResourceType Resource;
    public float Value;

    public CargoSaleResult(bool sold, ResourceType resource, float value)
    {
        Sold = sold;
        Resource = resource;
        Value = value;
    }

    public static CargoSaleResult None => new CargoSaleResult(false, default, 0f);
}