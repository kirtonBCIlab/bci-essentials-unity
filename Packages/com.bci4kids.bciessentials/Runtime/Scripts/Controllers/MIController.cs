using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.StimulusObjects;
using UnityEngine;
using UnityEngine.UI;

/*An extension of the controller class to add MI functionality

*/
public class MIController : Controller
{


    // Variables related to Iterative training
    public int numSelectionsBeforeTraining = 3;        // How many selections to make before creating the classifier
    public int numSelectionsBetweenTraining = 3;       // How many selections to make before updating the classifier


    protected int selectionCounter = 0;
    protected int updateCounter = 0;

    public override void PopulateObjectList(string popMethod)
    {
        base.PopulateObjectList(popMethod);

        // Warn about the number of objects to be selected from, if greater than 2
        if (objectList.Count > 2)
        {
            print("Warning: Selecting between more than 2 objects!");
        }
    }

    // Coroutine for the stimulus, wait there is no stimulus
    public override IEnumerator Stimulus()
    {
        yield return 0;
    }

    public override IEnumerator DoIterativeTraining()
    {
        // Generate the target list
        PopulateObjectList("tag");

        int numOptions = objectList.Count;

        // Create a random non repeating array 
        int[] trainArray = new int[numTrainingSelections];
        trainArray = MakeRNRA(numTrainingSelections, numOptions);
        PrintArray(trainArray);

        yield return 0;


        // Loop for each training target
        for (int i = 0; i < numTrainingSelections; i++)
        {

            if (selectionCounter >= numSelectionsBeforeTraining)
            {
                if (updateCounter == 0)
                {
                    // update the classifier
                    Debug.Log("Updating the classifier after " + selectionCounter.ToString() + " selections");

                    marker.Write("Update Classifier");
                    updateCounter++;
                }
                else if (selectionCounter >= numSelectionsBeforeTraining + (updateCounter * numSelectionsBetweenTraining))
                {
                    // update the classifier
                    Debug.Log("Updating the classifier after " + selectionCounter.ToString() + " selections");

                    marker.Write("Update Classifier");
                    updateCounter++;
                }
            }

            // Get the target from the array
            trainTarget = trainArray[i];

            // 
            Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

            // Turn on train target
            objectList[trainTarget].GetComponent<SPO>().OnTrainTarget();

            // Go through the training sequence
            yield return new WaitForSecondsRealtime(pauseBeforeTraining);


            StimulusOn();
            for (int j = 0; j < (numTrainWindows - 1); j++)
            {
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);

                if (shamFeedback)
                {
                    objectList[trainTarget].GetComponent<SPO>().Select();
                }
            }
            StimulusOff();

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);

            // Turn off train target
            objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();

            // Reset objects

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);

            trainTarget = 99;
            selectionCounter++;
        }

        // Send marker
        marker.Write("Training Complete");

        yield return 0;

    }

    public override IEnumerator SendMarkers(int trainingIndex = 99)
    {
        // Make the marker string, this will change based on the paradigm
        while (stimOn)
        {
            // Desired format is: [mi, number of options, training label (or -1 if n/a), window length] 
            string trainingString;
            if (trainingIndex <= objectList.Count)
            {
                trainingString = trainingIndex.ToString();
            }
            else
            {
                trainingString = "-1";
            }

            string markerString = "mi," + objectList.Count.ToString() + "," + trainingString + "," + windowLength.ToString();

            // Send the marker
            marker.Write(markerString);

            // Wait the window length + the inter-window interval
            yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);


        }
    }
}