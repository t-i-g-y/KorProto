using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArtifactInventoryEntry
{
    public string artifactId;
    public string title;
    public string story;
    public int day;
    public int hour;
}

[Serializable]
public class ArtifactPickupSaveData
{
    public string artifactId;
    public Vector3Int cell;
}

[Serializable]
public class ArtifactManagerSaveData
{
    public List<ArtifactInventoryEntry> inventory = new();
    public List<ArtifactPickupSaveData> activePickups = new();
    public List<string> spawnedArtifactIds = new();
}

