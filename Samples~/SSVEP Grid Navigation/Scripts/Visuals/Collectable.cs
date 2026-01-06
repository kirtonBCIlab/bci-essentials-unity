using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Collectable : GridSortedDisplay
{
    public float CollectionDelay = 0.2f;


    public void Collect()
    {
        StartCoroutine(RunCollectionDisplay());
    }
    
    private IEnumerator RunCollectionDisplay()
    {
        yield return new WaitForSeconds(CollectionDelay);
        Destroy(gameObject);
    }
}