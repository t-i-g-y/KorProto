using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public int maxTiles = 5;
    private List<GameObject> tiles = new List<GameObject>();
    private float railLength = 1;

    void Start()
    {
        AddTile(Vector3.zero);
        AddTile(4 * Vector3.right);
    }

    void AddTile(Vector3 position)
    {
        if (tiles.Count < maxTiles)
        {
            GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
            tiles.Add(newTile);
            PositionTiles();
        }
    }

    void RemoveTile()
    {
        if (tiles.Count > 2)
        {
            Destroy(tiles[tiles.Count - 1]);
            tiles.RemoveAt(tiles.Count - 1);
            PositionTiles();
        }
    }

    void PositionTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            tiles[i].transform.position = new Vector3(i, 0, 0);
        }
    }
}
