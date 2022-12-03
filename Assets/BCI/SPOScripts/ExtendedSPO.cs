using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedSPO : SPO
{
    // Start is called before the first frame update
    public override float TurnOn()
    {
        // Make Big
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue, objectScale.z * scaleValue);

        // Don't touch this
        // Return time since stim
        return Time.time;

    }

    public override void TurnOff()
    {
        // Make Small
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x / scaleValue, objectScale.y / scaleValue, objectScale.z / scaleValue);

    }

    public override void OnSelection()
    {
        // Blow Up
        Destroy(transform.gameObject);
    }
}
