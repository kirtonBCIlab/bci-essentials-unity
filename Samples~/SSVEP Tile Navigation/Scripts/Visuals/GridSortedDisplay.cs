using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GridSortedDisplay : MonoBehaviour
{
    protected SpriteRenderer Renderer
    => _renderer ? _renderer
    : _renderer = GetComponent<SpriteRenderer>();
    protected SpriteRenderer _renderer;

    public void UpdateSortOrder(Vector3Int gridPosition)
    => Renderer.sortingOrder = -(gridPosition.x + gridPosition.y);
}