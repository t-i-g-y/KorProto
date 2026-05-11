using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Rail/Visual Set")]
public class RailVisualSet : ScriptableObject
{
    public TileBase[] StartTiles = new TileBase[6];
    public TileBase[] EndTiles = new TileBase[6];
    public TileBase[] SelectedStartTiles = new TileBase[6];
    public TileBase[] SelectedEndTiles = new TileBase[6];
}
