using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FinanceSystemTests
{
    private GameObject financeGO;
    private FinanceSystem finance;

    [SetUp]
    public void SetUp()
    {
        financeGO = new GameObject();
        finance = financeGO.AddComponent<FinanceSystem>();

        TestImmitationHelper.InvokePrivateMethod(finance, "Initialize");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(financeGO);
    }

    [Test]
    public void DefaultStateTest()
    {
        Assert.AreEqual(100f, finance.Balance);
        Assert.AreEqual(0f, finance.LastBalanceChange);
        Assert.AreEqual(0f, finance.DayBalance);
        Assert.AreEqual(0, finance.CurrentDay);
    }

    [Test]
    public void BalanceChangeTest()
    {
        finance.AdjustBalance(25f);

        Assert.AreEqual(125f, finance.Balance);
        Assert.AreEqual(25f, finance.LastBalanceChange);
        Assert.AreEqual(25f, finance.DayBalance);
    }

    [Test]
    public void PayConstructionTest()
    {
        finance.PayConstruction(30f);

        Assert.AreEqual(70f, finance.Balance);
        Assert.AreEqual(-30f, finance.LastBalanceChange);
        Assert.AreEqual(-30f, finance.DayBalance);
    }

    [Test]
    public void NegativePayConstructionTest()
    {
        finance.PayConstruction(-30f);

        Assert.AreEqual(100f, finance.Balance);
        Assert.AreEqual(0f, finance.LastBalanceChange);
        Assert.AreEqual(0f, finance.DayBalance);
    }

    [Test]
    public void ApplyRefundTest()
    {
        finance.ApplyRefund(40f);

        Assert.AreEqual(140f, finance.Balance);
        Assert.AreEqual(40f, finance.LastBalanceChange);
        Assert.AreEqual(40f, finance.DayBalance);
    }

    [Test]
    public void DeductMaintenanceCostTest()
    {
        finance.DeductMaintenanceCost(15f);

        Assert.AreEqual(85f, finance.Balance);
        Assert.AreEqual(-15f, finance.LastBalanceChange);
        Assert.AreEqual(-15f, finance.DayBalance);
    }

    [Test]
    public void CargoValueMissingConfigTest()
    {
        ResourceType resource = FirstResourceType();

        float value = finance.GetCargoValue(resource);

        Assert.AreEqual(0f, value);
    }

    [Test]
    public void SellResourceBalanceChangeTest()
    {
        ResourceType resource = FirstResourceType();

        EconomyConfig config = ScriptableObject.CreateInstance<EconomyConfig>();
        config.CargoValues.Add(new ResourceValueEntry
        {
            Resource = resource,
            Value = 37f
        });

        TestImmitationHelper.SetPrivateField(finance, "economyConfig", config);

        float soldValue = finance.SellResource(resource);

        Assert.AreEqual(37f, soldValue);
        Assert.AreEqual(137f, finance.Balance);
        Assert.AreEqual(37f, finance.LastBalanceChange);
        Assert.AreEqual(37f, finance.DayBalance);
    }

    private static ResourceType FirstResourceType()
    {
        return (ResourceType)System.Enum.GetValues(typeof(ResourceType)).GetValue(0);
    }
}
