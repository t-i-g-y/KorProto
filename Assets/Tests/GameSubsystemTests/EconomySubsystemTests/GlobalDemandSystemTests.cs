using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GlobalDemandSystemTests
{
    private GameObject demandGO;
    private GlobalDemandSystem demandSystem;

    [SetUp]
    public void SetUp()
    {
        demandGO = new GameObject();
        demandSystem = demandGO.AddComponent<GlobalDemandSystem>();

        TestImmitationHelper.InvokePrivateMethod(demandSystem, "EnsureTransitInitialized");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(demandGO);
    }

    [Test]
    public void AddStationTransitTest()
    {
        ResourceType resource = FirstResourceType();

        demandSystem.AddStationTransit(10, resource, 3, 99);

        Assert.AreEqual(3, demandSystem.GetStationTransitAmount(10, resource));
    }

    [Test]
    public void PeekStationTransitDestinationFirstDestTest()
    {
        ResourceType resource = FirstResourceType();

        demandSystem.AddStationTransit(10, resource, 2, 99);

        bool result = demandSystem.PeekStationTransitDestination(10, resource, out int destinationStationID);

        Assert.IsTrue(result);
        Assert.AreEqual(99, destinationStationID);
        Assert.AreEqual(2, demandSystem.GetStationTransitAmount(10, resource));
    }

    [Test]
    public void TakeStationTransitTest()
    {
        ResourceType resource = FirstResourceType();

        demandSystem.AddStationTransit(10, resource, 2, 99);

        bool result = demandSystem.TakeStationTransitOne(10, resource, out int destinationStationID);

        Assert.IsTrue(result);
        Assert.AreEqual(99, destinationStationID);
        Assert.AreEqual(1, demandSystem.GetStationTransitAmount(10, resource));
    }

    [Test]
    public void TakeStationTransitOneEmptyStorageTest()
    {
        ResourceType resource = FirstResourceType();

        bool result = demandSystem.TakeStationTransitOne(10, resource, out int destinationStationID);

        Assert.IsFalse(result);
        Assert.AreEqual(-1, destinationStationID);
    }

    [Test]
    public void AddStationTransitNegativeAmountTest()
    {
        ResourceType resource = FirstResourceType();

        demandSystem.AddStationTransit(10, resource, 0, 99);
        demandSystem.AddStationTransit(10, resource, -1, 99);

        Assert.AreEqual(0, demandSystem.GetStationTransitAmount(10, resource));
    }

    private static ResourceType FirstResourceType()
    {
        return (ResourceType)System.Enum.GetValues(typeof(ResourceType)).GetValue(0);
    }
}