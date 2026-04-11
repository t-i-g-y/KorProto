using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RailManagerSaveData
{
    public int nextID;
    public int? selectedLineID;
    public List<RailLineSaveData> lines = new();
}