using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Collectable : GridSortedDisplay
{
    public float CollectionDelay = 0.2f;

    public void Collect() => Destroy(gameObject, CollectionDelay);
}