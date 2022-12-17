using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.Text;
using System.Linq;

public class P300Controller : Controller
{

    public int numFlashesLowerLimit = 9;
    public int numFlashesUpperLimit = 12;
    public System.Random randNumFlashes = new System.Random();
    private int numFlashesPerObjectPerSelection = 3;

    public float onTime = 0.2f;
    public float offTime = 0.3f;

    public bool singleFlash = true;
    public bool multiFlash = false;

    public bool rowColumn = false;
    public bool checkerboard = true;
    public int checkerBoardRows = 5;
    public int checkerBoardCols = 6;

    private float timeOfFlash = 0;
    private float timeOfWrite = 0;
    private float oldTimeOfWrite = 0;
    private float timeLag = 0;

    public bool timeDebug = false;

    private bool blockOutGoingLSL = false;

    public float trainBufferTime = 0f;


    public override IEnumerator DoTraining()
    {
        numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
        UnityEngine.Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

        // Generate the target list
        PopulateObjectList("tag");

        // Get number of selectable objects by counting the objects in the objectList
        int numOptions = objectList.Count;

        // Create a random non repeating array 
        int[] trainArray = new int[numTrainingSelections];
        trainArray = ArrayUtilities.GenerateRNRA(numTrainingSelections, numOptions);
        PrintArray(trainArray);

        yield return null;

        //System.Random randNumFlashes = new System.Random();

        // Loop for each training target
        for (int i = 0; i < numTrainingSelections; i++)
        {
            numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            UnityEngine.Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

            // Get the target from the array
            trainTarget = trainArray[i];

            // 
            UnityEngine.Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

            // Turn on train target

            objectList[trainTarget].GetComponent<SPO>().OnTrainTarget();

            // Go through the training sequence
            yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

            if (trainTargetPersistent == false)
            {
                objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();
            }

            yield return new WaitForSecondsRealtime(0.5f);

            // Calculate the length of the trial
            float trialTime = (onTime + offTime) * (1f + (10f / refreshRate)) * (float)numFlashesPerObjectPerSelection * (float)objectList.Count;

            UnityEngine.Debug.Log("This trial will take ~" + trialTime.ToString() + " seconds");

            StimulusOn(false);
            yield return new WaitForSecondsRealtime(trialTime);
            yield return new WaitForSecondsRealtime(trainBufferTime);
            //stimulusOff();

            // If sham feedback is true, then show it
            if (shamFeedback)
            {
                objectList[trainTarget].GetComponent<SPO>().OnSelection();
            }

            // Turn off train target
            yield return new WaitForSecondsRealtime(0.5f);

            if (trainTargetPersistent == true)
            {
                objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();
            }

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);

            trainTarget = 99;
        }

        marker.Write("Training Complete");

    }

    public override IEnumerator DoUserTraining()
    {
        numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
        UnityEngine.Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

        blockOutGoingLSL = true;

        // Generate the target list
        PopulateObjectList("tag");
        UnityEngine.Debug.Log("User Training");

        // Get a random training target
        trainTarget = randNumFlashes.Next(0, objectList.Count - 1);

        // Turn on train target

        objectList[trainTarget].GetComponent<SPO>().OnTrainTarget();

        // Go through the training sequence
        yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

        if (trainTargetPersistent == false)
        {
            objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // Calculate the length of the trial

        float trialTime = (onTime + offTime) * (1f + (10f / refreshRate)) * (float)numFlashesPerObjectPerSelection * (float)objectList.Count;

        UnityEngine.Debug.Log("This trial will take ~" + trialTime.ToString() + " seconds");

        StimulusOn(false);

        yield return new WaitForSecondsRealtime(trialTime);
        yield return new WaitForSecondsRealtime(trainBufferTime);
        //stimulusOff();

        // If sham feedback is true, then show it
        if (shamFeedback)
        {
            objectList[trainTarget].GetComponent<SPO>().OnSelection();
        }

        // Turn off train target
        yield return new WaitForSecondsRealtime(0.5f);

        if (trainTargetPersistent == true)
        {
            objectList[trainTarget].GetComponent<SPO>().OffTrainTarget();
        }

        // Take a break
        yield return new WaitForSecondsRealtime(trainBreak);

        trainTarget = 99;

        UnityEngine.Debug.Log("User Training Complete");

        blockOutGoingLSL = false;

        yield return null;
    }

    public override IEnumerator Stimulus()
    {
        numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
        UnityEngine.Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());
        // numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
        // UnityEngine.Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

        if (singleFlash)
        {
            int totalFlashes = numFlashesPerObjectPerSelection * objectList.Count;
            int[] stimOrder = ArrayUtilities.GenerateRNRA(totalFlashes, objectList.Count);

            for (int i = 0; i < stimOrder.Length; i++)
            {
                // 
                GameObject currentObject = objectList[stimOrder[i]];

                // Build the marker string
                StringBuilder markerStringBldr = new StringBuilder("p300,s,");
                markerStringBldr.Append(objectList.Count.ToString());

                if (trainTarget <= objectList.Count)
                {
                    markerStringBldr.AppendFormat(",{0}",trainTarget.ToString());
                }
                else
                {
                    markerStringBldr.Append(",-1");
                }
                markerStringBldr.AppendFormat(",{0}", stimOrder[i].ToString());

                UnityEngine.Debug.Log(markerStringBldr.ToString());

                timeOfFlash = currentObject.GetComponent<SPO>().TurnOn();


                //Send marker
                if (blockOutGoingLSL == false)
                {
                    marker.Write(markerStringBldr.ToString());
                }
                oldTimeOfWrite = timeOfWrite;
                timeOfWrite = Time.time;
                timeLag = timeOfWrite - oldTimeOfWrite;

                if (timeDebug)
                {
                    UnityEngine.Debug.Log("write - write lag:" + timeLag.ToString());
                }

                //stopwatch.Stop();
                //UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds.ToString());

                //Wait
                yield return new WaitForSecondsRealtime(onTime);

                //Turn off
                currentObject.GetComponent<SPO>().TurnOff();

                //Wait
                yield return new WaitForSecondsRealtime(offTime);
            }
        }
        if (multiFlash)
        {
            // Check if there are any real rows/columns



            // Assign object indices to places in the virtual row/column matrix
            //if (rcMethod.ToString() == "Ordered")
            //{
            if (rowColumn)
            {
                // For row/column flash selection, create virtual rows and columns
                int numSelections = objectList.Count;
                int numRows = (int)Math.Ceiling(Math.Sqrt((float)numSelections));
                int numColumns = (int)Math.Ceiling((float)numSelections / (float)numRows);

                UnityEngine.Debug.Log("This matrix is " + numRows.ToString() + " rows X " + numColumns.ToString() + "columns");

                int[,] rcMatrix = new int[numRows, numColumns];
                int count = 0;
                for (int i = 0; i < numRows; i++)
                {
                    for (int j = 0; j < numColumns; j++)
                    {
                        if (count <= numSelections)
                            rcMatrix[i, j] = count;
                        //print(i.ToString() + j.ToString() + count.ToString());
                        count++;
                    }
                }

                // Number of flashes per row/column
                int totalColumnFlashes = numFlashesPerObjectPerSelection * numColumns;
                int totalRowFlashes = numFlashesPerObjectPerSelection * numRows;

                // Create a random order to flash rows and columns
                int[] columnStimOrder = ArrayUtilities.GenerateRNRA(totalColumnFlashes, numColumns);
                int[] rowStimOrder = ArrayUtilities.GenerateRNRA(totalRowFlashes, numRows);

                // Repeat until the greater of rows/columns is completed
                
                for (int i = 0; i < Math.Max(totalColumnFlashes, totalRowFlashes); i++)
                {
                    // Flash column if available
                    if (i < totalColumnFlashes)
                    {
                        //Initialize marker string
                        StringBuilder markerStringBldr = new StringBuilder("p300,m,");
                        markerStringBldr.Append(objectList.Count.ToString());

                        //Add training target
                        if (trainTarget <= objectList.Count)
                        {
                            markerStringBldr.AppendFormat(",{0}", trainTarget.ToString());
                        }
                        else
                        {
                            markerStringBldr.Append(",-1");
                        }

                        // Turn on column 
                        int columnIndex = columnStimOrder[i];
                        for (int n = 0; n < numRows; n++)
                        {
                            try
                            {
                                GameObject currentObject = objectList[rcMatrix[n, columnIndex]];

                                //Add to marker
                                markerStringBldr.AppendFormat(",{0}", rcMatrix[n, columnIndex].ToString());

                                currentObject.GetComponent<SPO>().TurnOn();
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        // Send marker
                        if (blockOutGoingLSL == false)
                        {
                            marker.Write(markerStringBldr.ToString());
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(onTime);

                        //Turn off column
                        for (int n = 0; n < numRows; n++)
                        {
                            try
                            {
                                GameObject currentObject = objectList[rcMatrix[n, columnIndex]];
                                currentObject.GetComponent<SPO>().TurnOff();                            
                            }
                            catch
                            {
                                continue;
                            }

                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(offTime);
                    }

                    // Flash row if available
                    if (i < totalRowFlashes)
                    {
                        //Initialize marker string
                        StringBuilder markerStringBldr1 = new StringBuilder("p300,m,");
                        markerStringBldr1.Append(objectList.Count.ToString());

                        // Add training target
                        if (trainTarget <= objectList.Count)
                        {
                            markerStringBldr1.AppendFormat(",{0}", trainTarget.ToString());
                        }
                        else
                        {
                            markerStringBldr1.Append(",-1");
                        }

                        // Turn on row
                        int rowIndex = rowStimOrder[i];
                        for (int m = 0; m < numColumns; m++)
                        {
                            try
                            {
                                //Turn on row
                                GameObject currentObject = objectList[rcMatrix[rowIndex, m]];
                                currentObject.GetComponent<SPO>().TurnOn();

                                //Add to marker
                                markerStringBldr1.AppendFormat(",{0}", rcMatrix[rowIndex, m].ToString());
                            }
                            catch
                            {
                                continue;
                            }

                        }

                        //Send Marker
                        if (blockOutGoingLSL == false)
                        {
                            marker.Write(markerStringBldr1.ToString());
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(onTime);

                        //Turn off Row
                        for (int m = 0; m < numColumns; m++)
                        {
                            try
                            {
                                //Turn on row
                                GameObject currentObject = objectList[rcMatrix[rowIndex, m]];
                                currentObject.GetComponent<SPO>().TurnOff();
                            }
                            catch
                            {
                                continue;
                            }

                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(offTime);
                    }
                }
            }

            if (checkerboard)
            {
                // get the size of the black/white matrices
                double maxBWsize = Math.Ceiling(((float)checkerBoardRows * (float)checkerBoardCols) / 2f);

                // get the number of rows and columns
                int bwCols = (int)Math.Ceiling(Math.Sqrt(maxBWsize));
                int bwRows = (int)Math.Ceiling(Math.Sqrt(maxBWsize));
                // check if cbRows needs to match cbCols or if we can remove a row
                if (maxBWsize < ((bwRows * bwCols) - bwRows))
                {
                    bwRows = bwRows - 1;
                }

                int realBWSize = bwCols * bwRows;

                int[] blackList = new int[realBWSize];
                int[] whiteList = new int[realBWSize];

                int blackCount = 0;
                int whiteCount = 0;

                UnityEngine.Debug.Log("There are " + bwRows.ToString() + " rows and " + bwCols.ToString() + " columns in the BW matrices");

                System.Random rnd = new System.Random();
                int[] shuffledArray = Enumerable.Range(0, objectList.Count).OrderBy(c => rnd.Next()).ToArray();

                // assign from CB to BW
                for (int i = 0; i < objectList.Count; i++)
                {

                    // if there is an odd number of columns
                    if (checkerBoardCols % 2 == 1)
                    {
                        //evens assigned to black
                        if (shuffledArray[i] % 2 == 0)
                        {
                            blackList[blackCount] = shuffledArray[i];
                            blackCount++;
                        }
                        //odds assigned to white
                        else
                        {
                            whiteList[whiteCount] = shuffledArray[i];
                            whiteCount++;
                        }
                    }
                    // if there is an even number of columns
                    if (checkerBoardCols % 2 == 0)
                    {
                        //assigned to black
                        int numR = shuffledArray[i] / checkerBoardCols;
                        // print("to place" + shuffledArray[i].ToString());
                        // print("row number" + numR.ToString());

                        if (((shuffledArray[i] - (numR % 2)) % 2) == 0)
                        {
                            blackList[blackCount] = shuffledArray[i];
                            // print(shuffledArray[i] + " is black");
                            blackCount++;
                        }
                        //assigned to white
                        else
                        {
                            whiteList[whiteCount] = shuffledArray[i];
                            // print(shuffledArray[i] + " is white");
                            whiteCount++;
                        }
                    }

                }

                // set the remaining values to 99
                while (whiteCount < realBWSize)
                {
                    whiteList[whiteCount] = 99;
                    whiteCount++;
                }

                // set the remaining values to 99
                while (blackCount < realBWSize)
                {
                    blackList[blackCount] = 99;
                    blackCount++;
                }

                // Print the white and black indices
                UnityEngine.Debug.Log("blacks");
                PrintArray(blackList);
                UnityEngine.Debug.Log("whites");
                PrintArray(whiteList);

                // reshape the black and white arrays to 2D
                int[,] blackMat = new int[bwRows, bwCols];
                int[,] whiteMat = new int[bwRows, bwCols];

                int count = 0;
                for (int i = 0; i < bwRows; i++)
                {
                    for (int j = 0; j < bwCols; j++)
                    {
                        print(count.ToString());
                        blackMat[i, j] = blackList[count];
                        whiteMat[i, j] = whiteList[count];

                        count++;
                    }
                }

                int[] objectsToFlash = new int[bwCols];

                // for flash count
                for (int f = 0; f < numFlashesPerObjectPerSelection; f++)
                {
                    // for black rows
                    for (int br = 0; br < bwRows; br++)
                    {
                        for (int c = 0; c < bwCols; c++)
                        {
                            objectsToFlash[c] = blackMat[br, c];
                        }

                        PrintArray(objectsToFlash);

                        //Initialize marker string
                        string markerString1 = "p300,m," + objectList.Count.ToString();

                        // Add training target
                        if (trainTarget <= objectList.Count)
                        {
                            markerString1 = markerString1 + "," + trainTarget.ToString();
                        }
                        else
                        {
                            markerString1 = markerString1 + "," + "-1";
                        }

                        // Turn on objects to flash
                        for (int fi = 0; fi < bwCols; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOn();

                                //Add to marker
                                markerString1 = markerString1 + "," + objectsToFlash[fi].ToString();
                            }
                        }

                        //Send Marker
                        if (blockOutGoingLSL == false)
                        {
                            marker.Write(markerString1);
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(onTime);

                        //Turn off objects to flash
                        for (int fi = 0; fi < bwCols; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOff();
                            }
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(offTime);

                    }

                    // for white rows
                    for (int wr = 0; wr < bwRows; wr++)
                    {
                        for (int c = 0; c < bwCols; c++)
                        {
                            objectsToFlash[c] = whiteMat[wr, c];
                        }

                        PrintArray(objectsToFlash);

                        //Initialize marker string
                        string markerString1 = "p300,m," + objectList.Count.ToString();

                        // Add training target
                        if (trainTarget <= objectList.Count)
                        {
                            markerString1 = markerString1 + "," + trainTarget.ToString();
                        }
                        else
                        {
                            markerString1 = markerString1 + "," + "-1";
                        }

                        // Turn on objects to flash
                        for (int fi = 0; fi < bwCols; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOn();

                                //Add to marker
                                markerString1 = markerString1 + "," + objectsToFlash[fi].ToString();
                            }
                        }

                        //Send Marker
                        if (blockOutGoingLSL == false)
                        {
                            marker.Write(markerString1);
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(onTime);

                        //Turn off objects to flash
                        for (int fi = 0; fi < bwCols; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOff();
                            }
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(offTime);
                    }

                    // for black columns
                    for (int bc = 0; bc < bwCols; bc++)
                    {
                        for (int r = 0; r < bwRows; r++)
                        {
                            objectsToFlash[r] = blackMat[r, bc];
                        }

                        PrintArray(objectsToFlash);

                        //Initialize marker string
                        string markerString1 = "p300,m," + objectList.Count.ToString();

                        // Add training target
                        if (trainTarget <= objectList.Count)
                        {
                            markerString1 = markerString1 + "," + trainTarget.ToString();
                        }
                        else
                        {
                            markerString1 = markerString1 + "," + "-1";
                        }

                        // Turn on objects to flash
                        for (int fi = 0; fi < bwRows; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOn();

                                //Add to marker
                                markerString1 = markerString1 + "," + objectsToFlash[fi].ToString();
                            }
                        }

                        //Send Marker
                        if (blockOutGoingLSL == false)
                        {
                            marker.Write(markerString1);
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(onTime);

                        //Turn off objects to flash
                        for (int fi = 0; fi < bwRows; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOff();
                            }
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(offTime);
                    }

                    // for white columns
                    for (int wc = 0; wc < bwCols; wc++)
                    {
                        for (int r = 0; r < bwRows; r++)
                        {
                            objectsToFlash[r] = whiteMat[r, wc];
                        }

                        PrintArray(objectsToFlash);

                        //Initialize marker string
                        string markerString1 = "p300,m," + objectList.Count.ToString();

                        // Add training target
                        if (trainTarget <= objectList.Count)
                        {
                            markerString1 = markerString1 + "," + trainTarget.ToString();
                        }
                        else
                        {
                            markerString1 = markerString1 + "," + "-1";
                        }

                        // Turn on objects to flash
                        for (int fi = 0; fi < bwRows; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOn();

                                //Add to marker
                                markerString1 = markerString1 + "," + objectsToFlash[fi].ToString();
                            }
                        }

                        //Send Marker
                        if (blockOutGoingLSL == false)
                        {
                            marker.Write(markerString1);
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(onTime);

                        //Turn off objects to flash
                        for (int fi = 0; fi < bwRows; fi++)
                        {
                            if (objectsToFlash[fi] != 99)
                            {
                                //Turn on row
                                GameObject currentObject = objectList[objectsToFlash[fi]];
                                currentObject.GetComponent<SPO>().TurnOff();
                            }
                        }

                        //Wait
                        yield return new WaitForSecondsRealtime(offTime);
                    }
                }

                //
            }
            //}


        }
        StimulusOff();
    }

    public override IEnumerator SendMarkers(int trainTarget = 99)
    {
        // Do nothing, markers are are temporally bound to stimulus and are therefore sent from stimulus coroutine
        yield return null;
    }

    public override void StartStopStimulus()
    {
        // Receive incoming markers
        if (receivingMarkers == false)
        {
            StartCoroutine(ReceiveMarkers());
        }

        // P300 is special because the stimulus takes a fixed length of time 
        // and will cause issues if stopped prematurely

        // Turn off if on
        if (stimOn)
        {
            StimulusOff();
        }

        // Turn on if off
        else
        {
            PopulateObjectList("tag");
            StimulusOn();
        }
    }

    // Turn the stimulus on
    protected override void StimulusOn(bool sendConstantMarkers = true)
    {
        stimOn = true;

        // Send the marker to start
        if (blockOutGoingLSL == false)
        {
            marker.Write("Trial Started");
        }

        // Start the stimulus Coroutine
        try
        {
            StartCoroutine(Stimulus());

            // Not required for P300
            if (sendConstantMarkers)
            {
                StartCoroutine(SendMarkers(trainTarget));
            }
        }
        catch
        {
            UnityEngine.Debug.Log("start stimulus coroutine error");
        }
    }

    protected override void StimulusOff()
    {
        // End thhe stimulus Coroutine
        stimOn = false;

        // Send the marker to end
        if (blockOutGoingLSL == false)
        {
            marker.Write("Trial Ends");
        }
    }
}
