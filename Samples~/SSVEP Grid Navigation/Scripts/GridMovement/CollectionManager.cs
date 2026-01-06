using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class CollectionManager : MonoBehaviour
{
    private Tilemap Tiles
    => _tiles ? _tiles
    : _tiles = GetComponent<Tilemap>();
    private Tilemap _tiles;


    private void Start()
    {
        InitializeTiles();
        MovementHandler.MovementAchieved += TryCollection;
    }

    private void OnDestroy()
    {
        MovementHandler.MovementAchieved -= TryCollection;
    }


    private void InitializeTiles()
    {
        BoundsInt bounds = Tiles.cellBounds;

        foreach (Vector3Int gridPosition in bounds.allPositionsWithin)
        {
            if (!Tiles.HasTile(gridPosition)) continue;

            GameObject collectableObject = Tiles.GetInstantiatedObject(gridPosition);
            if (collectableObject.TryGetComponent(out Collectable collectable))
            {
                collectable.UpdateSortOrder(gridPosition);
            }
        }
    }


    private void TryCollection(Vector3Int gridPosition)
    {
        if (Tiles.HasTile(gridPosition))
        {
            GameObject collectableObject = Tiles.GetInstantiatedObject(gridPosition);
            if (collectableObject.TryGetComponent(out Collectable collectable))
            {
                collectable.Collect();
            }
            Tiles.SetTile(gridPosition, null);
        }
    }
}