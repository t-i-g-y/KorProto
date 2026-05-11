using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrainManagerSaveData
{
    public int nextID;
    public int selectedTrainID;
    public List<TrainSaveData> trains = new();
}
