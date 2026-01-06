using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TraversableTilemap : MonoBehaviour
{private Tilemap _tiles;

    private void Reset()
    {
        _tiles ??= GetComponent<Tilemap>();
    }


    public bool CanMoveTo(Vector3Int gridPosition)
    => _tiles.HasTile(gridPosition);

    public Vector3 GetCellCentre(Vector3Int cellPosition)
    => _tiles.GetCellCenterWorld(cellPosition);
}