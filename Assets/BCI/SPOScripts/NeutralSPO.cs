using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeutralSPO : SPO
{ 
    // What to do when targeted for training selection
    public override void OnTrainTarget()
    {
        // Turn everything else off
        GameObject[] objectArray = GameObject.FindGameObjectsWithTag("BCI");
        for (int i = 0; i < objectArray.Length; i++)
        {
            //objectArray[i].GetComponent<SPOMaterial>().color.a = 0f;

            Color tempColor = objectArray[i].GetComponent<Renderer>().material.color;
            tempColor.a = 0f;
            objectArray[i].GetComponent<Renderer>().material.color = tempColor;
            //stimImage.color = tempColor;
        }
    }

    public override void OffTrainTarget()
    {
        // Turn everything else off
        GameObject[] objectArray = GameObject.FindGameObjectsWithTag("BCI");
        for (int i = 0; i < objectArray.Length; i++)
        {
            //objectArray[i].GetComponent<SPOMaterial>().color.a = 0f;

            Color tempColor = objectArray[i].GetComponent<Renderer>().material.color;
            tempColor.a = 1f;
            objectArray[i].GetComponent<Renderer>().material.color = tempColor;
            //stimImage.color = tempColor;
        }
    }

    //private IEnumerator breathingPulse(float duration)
    //{
    //    float elapsedTime = 0f;
    //    float scaleFactor = 1;
    //    float scaleMax = 2;

    //    Vector3 originalScale = this.transform.localScale;

    //    while (elapsedTime < duration)
    //    {
    //        elapsedTime += Time.deltaTime;


    //        yield return null;
    //    }

    //    this.transform.localScale = new Vector3(1f, 1f, 1f);
    //}

}
