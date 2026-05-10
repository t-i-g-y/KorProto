using UnityEngine;

[CreateAssetMenu(menuName="Train/TrainConfig")]
public class TrainConfig : ScriptableObject
{
    public float TimePerLoadSec = 0.5f;
    public float TimePerUnloadSec = 0.5f;
    public float BreakChancePerSecond = 0.0005f;
}