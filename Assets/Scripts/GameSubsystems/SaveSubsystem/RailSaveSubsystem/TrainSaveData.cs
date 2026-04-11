using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrainSaveData
{
    public int ID;
    public int assignedLineID;
    public int speedLevel;
    public int dir;
    public int currentTileIndex;
    public float headDistance;
    public bool atStation;
    public bool isOperational;
    public TrainConsistSaveData consist;
}