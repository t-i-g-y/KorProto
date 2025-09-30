using System;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalDemand
{
    public static readonly ResourceAmount[] Outstanding = new ResourceAmount[]
    {
        new ResourceAmount(ResourceType.Circle),
        new ResourceAmount(ResourceType.Triangle),
        new ResourceAmount(ResourceType.Square) 
    };
}
