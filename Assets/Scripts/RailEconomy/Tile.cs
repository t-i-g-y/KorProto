using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public enum TerrainType { Forest, Jungle, Desert, Tundra, Hills, Grassland }
    public enum EndpointType { None, SmallCity, City, LargeCity, Factory, Farm, Port, Airport, LogisticsHub, Tourism }

    public TerrainType terrainType;
    public EndpointType endpointType;
    public Button terrainButton;
    public Button endpointButton;

    private void Start()
    {
        terrainButton.onClick.AddListener(ChangeTerrainType);
        endpointButton.onClick.AddListener(ChangeEndpointType);
    }

    void ChangeTerrainType()
    {
        terrainType = (TerrainType)(((int)terrainType + 1) % System.Enum.GetValues(typeof(TerrainType)).Length);
        UpdateTileVisuals();
    }

    void ChangeEndpointType()
    {
        endpointType = (EndpointType)(((int)endpointType + 1) % System.Enum.GetValues(typeof(EndpointType)).Length);
        UpdateTileVisuals();
    }

    void UpdateTileVisuals()
    {

    }

    public float CalculateProfit() => CalculateIncome() - CalculateExpenses();
    private float CalculateExpenses()
    {
        float expenses = 0;
        switch (terrainType)
        {
            case TerrainType.Forest:
                expenses += 2;
                break;
            case TerrainType.Jungle:
                expenses += 3;
                break;
            case TerrainType.Desert:
                expenses += 1;
                break;
            case TerrainType.Tundra:
                expenses += 4;
                break;
            case TerrainType.Hills:
                expenses += 5;
                break;
            case TerrainType.Grassland:
                expenses += 1;
                break;
        }


        return expenses;
    }

    private float CalculateIncome()
    {
        float income = 0;
        switch (endpointType)
        {
            case EndpointType.SmallCity:
                income += 1;
                break;
            case EndpointType.City:
                income += 2;
                break;
            case EndpointType.LargeCity:
                income += 3;
                break;
            case EndpointType.Factory:
                income += 4;
                break;
            case EndpointType.Farm:
                income += 2;
                break;
            case EndpointType.Port:
                income += 5;
                break;
            case EndpointType.Airport:
                income += 6;
                break;
            case EndpointType.LogisticsHub:
                income += 7;
                break;
            case EndpointType.Tourism:
                income += 3;
                break;
        }

        return income;
    }
}

