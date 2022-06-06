using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralSPO : SPO
{ 
    // What to do on selection
    public override void OnSelection()
    {
        // Relaxing pulse around breathing rate
        float durationOfPulse = 1f;
        StartCoroutine(breathingPulse(durationOfPulse));
    }

    //
    // What to do when targeted for training selection
    public override void OnTrainTarget()
    {
        float scaleValue = 1.4f;
        Vector3 objectScale = transform.localScale;
        transform.localScale = new Vector3(objectScale.x * scaleValue, objectScale.y * scaleValue, objectScale.z * scaleValue);

        // Relaxing pulse around breathing rate
        float durationOfPulse = 1f;
        StartCoroutine(breathingPulse(durationOfPulse));
    }

    private IEnumerator breathingPulse(float duration)
    {
        float elapsedTime = 0f;
        float scaleFactor = 1;
        float scaleMax = 2;

        Vector3 originalScale = this.transform.localScale;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;


            yield return null;
        }

        this.transform.localScale = new Vector3(1f, 1f, 1f);
    }

}
