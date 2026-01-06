using System;
using UnityEngine;

[RequireComponent(typeof(TraversableTilemap))]
public class MovementHandler : MonoBehaviour
{
    public static event Action<Vector3Int> MovementAchieved;
    public static event Action MovementBlocked;

    public static Vector3Int GridPosition { get; private set; }

    private TraversableTilemap Map
    => _map ? _map
    : _map = GetComponent<TraversableTilemap>();
    private TraversableTilemap _map;


    public void Move(Vector2Int direction)
    {
        Vector3Int targetPosition = GridPosition + (Vector3Int)direction;

        if (Map.CanMoveTo(targetPosition))
        {
            GridPosition = targetPosition;
            MovementAchieved?.Invoke(GridPosition);
        }
        else MovementBlocked?.Invoke();
    }


    public void MoveNorthWest() => Move(Vector2Int.up);
    public void MoveSouthEast() => Move(Vector2Int.down);
    public void MoveNorthEast() => Move(Vector2Int.right);
    public void MoveSouthWest() => Move(Vector2Int.left);
}