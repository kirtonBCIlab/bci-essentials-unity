using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSPO : SPO
{
    //Threshold
    public float threshold = 0.5f;

    //Height 
    public float minHeight = 0f;
    public float maxHeight = 4f;

    //Time to rise
    public float timeToRise = 4f;

    
    // What to do with the activation value, between 0 and 1
    public void OnActivation(float activationValue)
    {
        // Do an action corresponding to activation level

        // If the activation value is above the threshold then do something else
        if(activationValue >= threshold)
        {
            OnSelection();
        }

        // Reset
        TurnOff();
    }

    // What to do on selection
    public override void OnSelection()
    {
        StartCoroutine(QuickFlash());

        // Reset
        TurnOff();
    }

    // What to do when targeted for training selection
    public override void OnTrainTarget()
    {
        StartCoroutine(MoveUp(maxHeight, timeToRise));
    }

    // What to do when untargeted
    public override void OffTrainTarget()
    {
        ResetPosition();
    }

    public IEnumerator MoveUp(float targetHeight, float duration)
    {
        float elapsedTime = 0;
        float distanceToMove;


        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            distanceToMove = targetHeight * (Time.deltaTime / duration);

            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + distanceToMove, this.transform.position.z);

            // Wait for next frame
            yield return 0;
        }

        ResetPosition();
    }

    public void ResetPosition()
    {
        this.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
    }
}
