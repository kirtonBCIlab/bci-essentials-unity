using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P300Controller : Controller
{
    public int numFlashesPerObjectPerSelection = 3;


    public float onTime = 0.2f;
    public float offTime = 0.3f;

    public bool singleFlash = true;

    public override IEnumerator doTraining()
    {
        // Generate the target list
        populateObjectList("tag");

        // Get number of selectable objects by counting the objects in the objectList
        int numOptions = objectList.Count;

        // Create a random non repeating array 
        int[] trainArray = new int[numTrainingSelections];
        trainArray = makeRNRA(numTrainingSelections, numOptions);
        printArray(trainArray);

        yield return new WaitForSecondsRealtime(0.001f);

        // Loop for each training target
        for (int i = 0; i < numTrainingSelections; i++)
        {
            // Get the target from the array
            trainTarget = trainArray[i];

            // 
            Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

            // Turn on train target
            objectList[trainTarget].GetComponent<SPO>().onTrainTarget();

            // Go through the training sequence
            yield return new WaitForSecondsRealtime(3f);

            // Calculate the length of the trial
            float trialTime = (onTime + offTime) * (float)numFlashesPerObjectPerSelection * (float)objectList.Count;

            stimulusOn();
            yield return new WaitForSecondsRealtime(trialTime);
            stimulusOff();

            // Turn off train target
            objectList[trainTarget].GetComponent<SPO>().offTrainTarget();

            // If sham feedback is true, then show it
            if (shamFeedback)
            {
                objectList[trainTarget].GetComponent<SPO>().onSelection();
            }

            trainTarget = 99;

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);
        }
    }

    public override IEnumerator stimulus()
    {
        if (singleFlash)
        {
            int totalFlashes = numFlashesPerObjectPerSelection * objectList.Count;
            int[] stimOrder = makeRNRA(totalFlashes, objectList.Count);

            for (int i = 0; i < stimOrder.Length; i++)
            {
                // 
                GameObject currentObject = objectList[stimOrder[i]];

                //Turn on
                currentObject.GetComponent<SPO>().turnOn();
                string markerString = "s," + stimOrder[i].ToString();

                if (trainTarget <= objectList.Count)
                {
                    markerString = markerString + "," + trainTarget.ToString();
                }
                marker.Write(markerString);

                //Wait
                yield return new WaitForSecondsRealtime(onTime);

                //Turn off
                currentObject.GetComponent<SPO>().turnOff();

                //Wait
                yield return new WaitForSecondsRealtime(offTime);
            }
        }
    }

    public override IEnumerator sendMarkers(int trainTarget=99)
    {
        // Do nothing, markers are are temporally bound to stimulus and are therefore sent from stimulus coroutine
        yield return new WaitForSecondsRealtime(0.001f);
    }
}
