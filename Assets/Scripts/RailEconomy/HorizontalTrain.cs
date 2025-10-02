using UnityEngine;

public class HorizontalTrain : MonoBehaviour
{
    public Tile startTile;
    public Tile endTile;
    public EconomyManager economyManager;

    private void Start()
    {
        MoveTrain();
    }

    void MoveTrain()
    {
        float distance = Vector3.Distance(startTile.transform.position, endTile.transform.position);
        float profit = startTile.CalculateProfit() + endTile.CalculateProfit() - distance;

        economyManager.totalProfit += profit;
        Debug.Log("Train Reached! Profit Generated: " + profit);
    }
}
