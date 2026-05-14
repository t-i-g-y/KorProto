using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DemandRequestTests
{
    [Test]
    public void DemandRequestConstructorTest()
    {
        ResourceType resource = FirstResourceType();

        DemandRequest request = new DemandRequest(28, resource, 3);

        Assert.AreEqual(28, request.StationID);
        Assert.AreEqual(resource, request.Resource);
        Assert.AreEqual(3, request.Amount);
        Assert.IsFalse(request.IsEmpty);
    }

    [Test]
    public void EmptyDemandRequestTest()
    {
        ResourceType resource = FirstResourceType();

        Assert.IsTrue(new DemandRequest(1, resource, 0).IsEmpty);
        Assert.IsTrue(new DemandRequest(1, resource, -333).IsEmpty);
    }
    private static ResourceType FirstResourceType()
    {
        return (ResourceType)System.Enum.GetValues(typeof(ResourceType)).GetValue(0);
    }
}
