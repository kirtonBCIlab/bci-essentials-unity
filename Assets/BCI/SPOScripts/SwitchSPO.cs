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

    //Controller
    public GameObject controller;

    void Start()
    {
        controller = GameObject.Find("SwitchController");
        //private GameObject controller = GameObject.Find("SwitchController");
    }

    // What to do with the activation value, between 0 and 1
    public void OnActivation(float activationValue, float activationDuration)
    {
        // Do an action corresponding to activation level
        // Go to height
        float activationHeight = maxHeight * activationValue;
        StartCoroutine(MoveUp(activationHeight, activationDuration, false));

        // If the activation value is above the threshold then do something else
        if(activationValue >= threshold)
        {
            this.GetComponent<Renderer>().material.color = onColour;
        }
        // else change it back
        else
        {
            this.GetComponent<Renderer>().material.color = offColour;
        }

        // Reset
        // TurnOff();
    }

    // What to do on selection
    public override void OnSelection()
    {
        StartCoroutine(QuickFlash());

        // Reset
        // TurnOff();
    }

    // What to do when targeted for training selection
    public override void OnTrainTarget()
    {
        // Make the other targets turn off 
        GameObject[] objectArray = GameObject.FindGameObjectsWithTag("BCI");
        for (int i = 0; i < objectArray.Length; i++)
        {
            // turn off all the other SPOs
            if (i != myIndex)
            {
                Color tempColor = objectArray[i].GetComponent<Renderer>().material.color;
                tempColor.a = 0f;
                objectArray[i].GetComponent<SwitchSPO>().ResetPosition();
                objectArray[i].GetComponent<Renderer>().material.color = tempColor;
            }

        }

        // Return sham or feedback

        ResetPosition();

        // if sham feedback
        bool shamFeedback;
        shamFeedback = controller.GetComponent<SwitchController>().shamFeedback;
        if(shamFeedback)
        {
            StartCoroutine(MoveUp(maxHeight, timeToRise, true));
        }
    }

    // What to do when untargeted
    public override void OffTrainTarget()
    {
        GameObject[] objectArray = GameObject.FindGameObjectsWithTag("BCI");
        for (int i = 0; i < objectArray.Length; i++)
        {
            // turn on all the other SPOs
            if (i != myIndex)
            {
                Color tempColor = objectArray[i].GetComponent<Renderer>().material.color;
                tempColor.a = 1f;
                objectArray[i].GetComponent<Renderer>().material.color = tempColor;
            }
        }

        ResetPosition();
    }

    public IEnumerator MoveUp(float targetHeight, float duration, bool resetPos)
    {
        float elapsedTime = 0;
        float totalDistanceToMove;
        float distanceToMove;

        Debug.Log("moving up " + targetHeight.ToString() + " units over " + duration.ToString() + " seconds");
        Debug.Log("initial position " + this.transform.position.y);

        //ResetPosition();
        totalDistanceToMove = targetHeight - this.transform.position.y;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            distanceToMove = totalDistanceToMove * (Time.deltaTime / duration);

            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + distanceToMove, this.transform.position.z);

            // Wait for next frame
            yield return 0;
        }

        Debug.Log("final position " + this.transform.position.y);

        if (resetPos)
        {
            ResetPosition();
        }
        //ResetPosition();
    }

    public void ResetPosition()
    {
        this.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
    }
}
