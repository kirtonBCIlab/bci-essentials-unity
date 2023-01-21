using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Use Stimulus Effects instead.")]
public class MITrainingSPO : SPO
{
    private Vector3 originalPosition;

    // Start is called before the first frame update
    public override float StartStimulus()
    {
        // Record current location
        originalPosition = transform.position;

        // Don't touch this
        // Return time since stim
        return Time.time;

    }

    public override void StopStimulus()
    {
        // Reset to Original
        transform.position = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z);

    }

    public override void Select()
    {
        // Move up a little bit
        Vector3 myCurrentPosition = transform.position;
        Vector3 newPosition = new Vector3(myCurrentPosition.x, myCurrentPosition.y + 0.2f, myCurrentPosition.z);
    }
}
