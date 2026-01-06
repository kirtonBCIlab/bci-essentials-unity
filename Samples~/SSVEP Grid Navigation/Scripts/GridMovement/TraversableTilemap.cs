using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TraversableTilemap : MonoBehaviour
{
    private Tilemap Tiles
    => _tiles ? _tiles
    : _tiles = GetComponent<Tilemap>();
    private Tilemap _tiles;

    public bool CanMoveTo(Vector3Int gridPosition)
    => Tiles.HasTile(gridPosition);

    public Vector3 GetCellCentre(Vector3Int cellPosition)
    => Tiles.GetCellCenterWorld(cellPosition);
}