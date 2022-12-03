using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MITrainingSPO : SPO
{
    private Vector3 originalPosition;

    // Start is called before the first frame update
    public override float TurnOn()
    {
        // Record current location
        originalPosition = transform.position;

        // Don't touch this
        // Return time since stim
        return Time.time;

    }

    public override void TurnOff()
    {
        // Reset to Original
        transform.position = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z);

    }

    public override void OnSelection()
    {
        // Move up a little bit
        Vector3 myCurrentPosition = transform.position;
        Vector3 newPosition = new Vector3(myCurrentPosition.x, myCurrentPosition.y + 0.2f, myCurrentPosition.z);


    }

    //private IEnumerator LerpMe(Vec)


}
