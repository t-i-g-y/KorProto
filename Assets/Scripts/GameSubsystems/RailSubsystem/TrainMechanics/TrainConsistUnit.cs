using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrainConsistUnit
{
    public int capacity;
    public List<ResourceAmount> cargo = new();
    public float maintenance = 1f;

    public TrainConsistUnit(int capacity, float maintenance = 1f)
    {
        this.capacity = Mathf.Max(0, capacity);
        this.maintenance = Mathf.Max(0f, maintenance);
    }
}
