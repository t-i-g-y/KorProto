using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CargoSaleResultTests
{
    [Test]
    public void CargoSaleResultConstructorTest()
    {
        ResourceType resource = FirstResourceType();

        CargoSaleResult result = new CargoSaleResult(true, resource, 25f);

        Assert.IsTrue(result.Sold);
        Assert.AreEqual(resource, result.Resource);
        Assert.AreEqual(25f, result.Value);
    }

    [Test]
    public void EmpyCargoSaleResultTest()
    {
        CargoSaleResult result = CargoSaleResult.None;

        Assert.IsFalse(result.Sold);
        Assert.AreEqual(0f, result.Value);
    }

    private static ResourceType FirstResourceType()
    {
        return (ResourceType)System.Enum.GetValues(typeof(ResourceType)).GetValue(0);
    }
}
