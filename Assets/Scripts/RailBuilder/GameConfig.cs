using UnityEngine;

[CreateAssetMenu(menuName="Rail/GameConfig")]
public class GameConfig : ScriptableObject
{
    public float SpawnEverySec = 6f;
    public int SpawnBatchMin = 1;
    public int SpawnBatchMax = 2;
    public float TimePerLoadSec = 0.5f;
    public float TimePerUnloadSec = 0.5f;
    public int StationSupplyCap = 20;
}
