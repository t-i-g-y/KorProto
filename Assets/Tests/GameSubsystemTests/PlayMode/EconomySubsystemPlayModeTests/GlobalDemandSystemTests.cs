using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GlobalDemandSystemPlayModeTests
{
    private GameObject demandGO;
    private GlobalDemandSystem demandSystem;

    private static ResourceType FirstResourceType()
    {
        return (ResourceType)System.Enum.GetValues(typeof(ResourceType)).GetValue(0);
    }

    [SetUp]
    public void SetUp()
    {
        demandGO = new GameObject();
        demandSystem = demandGO.AddComponent<GlobalDemandSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        if (demandGO != null)
            Object.DestroyImmediate(demandGO);
    }

    [UnityTest]
    public IEnumerator AwakeInitializesDemandStorage()
    {
        yield return null;
        ResourceType resource = FirstResourceType();

        Assert.IsNotNull(demandSystem.OutstandingTotals);
        Assert.IsNotNull(demandSystem.DemandRequests);
        Assert.IsTrue(demandSystem.DemandRequests.ContainsKey(resource));
        Assert.AreEqual(0, demandSystem.GetOutstanding(resource));
    }

    [UnityTest]
    public IEnumerator AddStationTransitStores()
    {
        yield return null;
        ResourceType resource = FirstResourceType();
        demandSystem.AddStationTransit(10, resource, 2, 99);

        Assert.AreEqual(2, demandSystem.GetStationTransitAmount(10, resource));
        Assert.IsTrue(demandSystem.PeekStationTransitDestination(10, resource, out int destination));
        Assert.AreEqual(99, destination);
    }

    [UnityTest]
    public IEnumerator TakeStationTransitOneConsumesOne()
    {
        yield return null;
        ResourceType resource = FirstResourceType();
        demandSystem.AddStationTransit(10, resource, 2, 99);
        bool taken = demandSystem.TakeStationTransitOne(10, resource, out int destination);

        Assert.IsTrue(taken);
        Assert.AreEqual(99, destination);
        Assert.AreEqual(1, demandSystem.GetStationTransitAmount(10, resource));
    }
}
