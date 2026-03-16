using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalDemand
{
    public static readonly ResourceAmount[] Outstanding = CreateOutstanding();

    private static ResourceAmount[] CreateOutstanding()
    {
        ResourceType[] resourceTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));
        ResourceAmount[] outstanding = new ResourceAmount[resourceTypes.Length];

        for (int index = 0; index < resourceTypes.Length; index++)
            outstanding[index] = new ResourceAmount(resourceTypes[index]);

        return outstanding;
    }
}
