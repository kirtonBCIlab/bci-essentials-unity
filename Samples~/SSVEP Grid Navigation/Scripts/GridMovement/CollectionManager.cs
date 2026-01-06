using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class CollectionManager : MonoBehaviour
{
    private Tilemap _tiles;

    private void Reset()
    {
        _tiles ??= GetComponent<Tilemap>();
    }


    private void Start()
    {
        MovementHandler.MovementAchieved += TryCollection;
    }

    private void OnDestroy()
    {
        MovementHandler.MovementAchieved -= TryCollection;
    }


    private void InitializeTiles()
    {
        BoundsInt bounds = _tiles.cellBounds;

        foreach (Vector3Int gridPosition in bounds.allPositionsWithin)
        {
            if (!_tiles.HasTile(gridPosition)) continue;

            GameObject collectableObject = _tiles.GetInstantiatedObject(gridPosition);
            if (collectableObject.TryGetComponent(out Collectable collectable))
            {
                collectable.UpdateSortOrder(gridPosition);
            }
        }
    }


    private void TryCollection(Vector3Int gridPosition)
    {
        if (_tiles.HasTile(gridPosition))
        {
            GameObject collectableObject = _tiles.GetInstantiatedObject(gridPosition);
            if (collectableObject.TryGetComponent(out Collectable collectable))
            {
                collectable.Collect();
            }
            _tiles.SetTile(gridPosition, null);
        }
    }
}