using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Trains")]
    public static GameObject TrainPrefab;
    public static List<GameObject> TrainPool = new();
}
