using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class P300Controller : Controller
{
    public int numFlashesPerObjectPerSelection = 3;


    public float onTime = 0.2f;
    public float offTime = 0.3f;

    public bool singleFlash = true;
    public bool multiFlash = false;

    public bool rowColumn = false;
    public bool checkerboard = true;

    public enum multiFlashMethod {Random};
    

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

        marker.Write("Training Complete");
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
        if (multiFlash)
        {
            // For multi flash selection, create virtual rows and columns
            int numSelections = objectList.Count;
            int numColumns = (int)Math.Ceiling(Math.Sqrt((float)numSelections));
            int numRows = (int)Math.Ceiling((float)numSelections / (float)numColumns);

            int[,] rcMatrix = new int[numColumns, numRows];

            // Assign object indices to places in the virtual row/column matrix
            //if (rcMethod.ToString() == "Ordered")
            //{
            if(rowColumn)
            {
                int count = 0;
                for (int i = 0; i < numColumns; i++)
                {
                    for (int j = 0; j < numRows; j++)
                    {
                        if (count <= numSelections)
                            rcMatrix[i, j] = count;
                        //print(i.ToString() + j.ToString() + count.ToString());
                        count++;
                    }
                }
            }

            if(checkerboard)
            {
                int count = 0;
                for (int i = 0; i < numColumns; i++)
                {
                    for (int j = 0; j < numRows; j++)
                    {
                        if (count <= numSelections)
                            rcMatrix[i, j] = count;
                        count++;
                    }
                }
            }
            //}

            // Number of flashes per row/column
            int totalColumnFlashes = numFlashesPerObjectPerSelection * numColumns;
            int totalRowFlashes = numFlashesPerObjectPerSelection * numRows;

            // Create a random order to flash rows and columns
            int[] columnStimOrder = makeRNRA(totalColumnFlashes, numColumns);
            int[] rowStimOrder = makeRNRA(totalRowFlashes, numRows);

            for (int i = 0; i < totalColumnFlashes; i++)
            {
                //Initialize marker string
                string markerString = "m";

                // Turn on column 
                int columnIndex = columnStimOrder[i];
                for (int n = 0; n < numRows; n++)
                {
                    GameObject currentObject = objectList[rcMatrix[n,columnIndex]];
                    currentObject.GetComponent<SPO>().turnOn();

                    //Add to marker
                    markerString = markerString + "," + rcMatrix[n,columnIndex].ToString();
                }

                // Add train target to marker
                if (trainTarget <= objectList.Count)
                {
                    markerString = markerString + "," + trainTarget.ToString();
                }

                // Send marker
                marker.Write(markerString);

                //Wait
                yield return new WaitForSecondsRealtime(onTime);

                //Turn off column
                for (int n = 0; n < numRows; n++)
                {
                    GameObject currentObject = objectList[rcMatrix[n, columnIndex]];
                    currentObject.GetComponent<SPO>().turnOff();
                }

                //Wait
                yield return new WaitForSecondsRealtime(offTime);

                // Flash row if available
                if (i <= totalRowFlashes)
                {
                    //Initialize marker string
                    string markerString1 = "m";

                    // Turn on row
                    int rowIndex = rowStimOrder[i];
                    for (int m = 0; m < numColumns; m++)
                    {
                        //Turn on row
                        GameObject currentObject = objectList[rcMatrix[rowIndex, m]];
                        currentObject.GetComponent<SPO>().turnOn();

                        //Add to marker
                        markerString1 = markerString1 + "," + rcMatrix[rowIndex, m].ToString();
                    }

                    //Add train target to marker
                    if (trainTarget <= objectList.Count)
                    {
                        markerString1 = markerString1 + "," + trainTarget.ToString();
                    }

                    //Send Marker
                    marker.Write(markerString1);

                    //Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    //Turn off Row
                    for (int m = 0; m < numColumns; m++)
                    {
                        //Turn on row
                        GameObject currentObject = objectList[rcMatrix[rowIndex, m]];
                        currentObject.GetComponent<SPO>().turnOff();
                    }


                    //Wait
                    yield return new WaitForSecondsRealtime(offTime);
                }
            }
        }
    }

    public override IEnumerator sendMarkers(int trainTarget=99)
    {
        // Do nothing, markers are are temporally bound to stimulus and are therefore sent from stimulus coroutine
        yield return new WaitForSecondsRealtime(0.001f);
    }
}
