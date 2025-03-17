using BCIEssentials.StimulusObjects;
using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using Random = System.Random;
using System.Collections.Generic;

namespace BCIEssentials.ControllerBehaviors
{
    public class P300ControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.P300;

        [Header("P300 Training Properties")]
        [Tooltip("The time between the end of a sequence and making a selection [sec]")]
        public float trainBufferTime = 0f;
        
        [FoldoutGroup("P300 Pattern Flashing Properties")]
        public int numFlashesLowerLimit = 9;
        public int numFlashesUpperLimit = 12;
        public Random randNumFlashes = new Random();
        private int numFlashesPerObjectPerSelection = 3;

        [Tooltip("[sec]")]
        public float onTime = 0.1f;
        [Tooltip("[sec]")]
        public float offTime = 0.075f;

        [FoldoutGroup("Stimulus Flash Paradigms")]
        [Header("Single Flash Properties")]
        [Tooltip("If true, only one SPO will flash at a time")]
        public bool singleFlash = true;

        [ShowIf("singleFlash")]
        [Tooltip("If true, enables context-aware SPO single flashing")]
        public bool contextAwareSingleFlash = false;

        [Header("Multi Flash Properties")]
        [Tooltip("If true, enables multiple SPOs to flash at the same time." +
        "Needs to be true for rowColumn and checkerboard to work")]
        public bool multiFlash = false;
        
        [ShowIf("multiFlash")]
        [Tooltip("If true, flashes objects in rows and columns. Requires Multiflash to be true")]
        public bool rowColumn = false;
        
        [ShowIf("multiFlash")]
        [Tooltip("If true, flashes objects in a checkerboard pattern. Requires Multiflash to be true")]
        public bool checkerboard = true;

        [ShowIf("multiFlash")]
        [Tooltip("If true, enables context-aware SPO multi flashing")]
        public bool contextAwareMultiFlash = false;

        [Header("Row/Column & Checkerboard Properties")]
        [ShowIf("multiFlash")]
        [Tooltip("Number of rows in multi-flash RowColumn or Checkerboard")]
        public int numFlashRows = 5;
        [ShowIf("multiFlash")]
        [Tooltip("Number of columns in the multi-flash RowColumn or Checkerboard")]
        public int numFlashColumns = 6;



        public enum multiFlashMethod
        {
            Random
        };

        private float timeOfFlash = 0;
        private float timeOfWrite = 0;
        private float oldTimeOfWrite = 0;
        private float timeLag = 0;

        [FoldoutGroup("Debugging Parameters", 12, 20)]
        public bool timeDebug = false;
        [EndFoldoutGroup]
        private bool blockOutGoingLSL = false;

        //I have updated the starting __uniqueP300ID to 0, as it was causing issues with the LSL markers at 1.
        private int __uniqueP300ID = 0;

        public SpoPopulationMethod myPopMethod = SpoPopulationMethod.GraphBP;

        private List<GameObject> _validGOs = new List<GameObject>();
        private int lastTourEndNode = -100;



        protected override IEnumerator WhileDoAutomatedTraining()
        {
            numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

            // Generate the target list
            PopulateObjectList();

            // Get number of selectable objects by counting the objects in the objectList
            int numOptions = _selectableSPOs.Count;

            // See how performant the Fisher-Yates shuffle is
            var watch = System.Diagnostics.Stopwatch.StartNew();
            // Create a random non repeating array 
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
            LogArrayValues(trainArray);
            watch.Stop();
            Debug.Log("Fisher-Yates shuffle took " + watch.ElapsedMilliseconds.ToString() + " milliseconds");
            // int[] weightedTrainArray = ArrayUtilities.GenerateWeightedArray(numTrainingSelections, 0, numOptions-1);
            // LogArrayValues(weightedTrainArray);
            // int[] fisherArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
            // LogArrayValues(fisherArray);

            yield return null;

            //System.Random randNumFlashes = new System.Random();

            // Loop for each training target
            for (int i = 0; i < numTrainingSelections; i++)
            {
                numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
                Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

                // Get the target from the array
                trainTarget = trainArray[i];

                // 
                Debug.Log("Running training selection " + i.ToString() + " on option " +
                          trainTarget.ToString());

                // Turn on train target

                _selectableSPOs[trainTarget].GetComponent<SPO>().OnTrainTarget();

                // Go through the training sequence
                yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

                if (trainTargetPersistent == false)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }

                yield return new WaitForSecondsRealtime(0.5f);

                // Start stimulus and wait for it to complete
                StartStimulusRun(false);
                yield return StartCoroutine(WaitForStimulusToComplete());
                yield return new WaitForSecondsRealtime(trainBufferTime);

                // If sham feedback is true, then show it
                if (shamFeedback)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().Select();
                }

                // Turn off train target
                yield return new WaitForSecondsRealtime(0.5f);

                if (trainTargetPersistent == true)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }

                // Take a break
                yield return new WaitForSecondsRealtime(trainBreak);

                trainTarget = 99;
            }

            OutStream.PushTrainingCompleteMarker();

        }

        protected override IEnumerator WhileDoUserTraining()
        {
            numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

            blockOutGoingLSL = true;

            // Generate the target list
            PopulateObjectList();
            Debug.Log("User Training");

            // Get a random training target
            trainTarget = randNumFlashes.Next(0, _selectableSPOs.Count - 1);

            // Turn on train target

            _selectableSPOs[trainTarget].GetComponent<SPO>().OnTrainTarget();

            // Go through the training sequence
            yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

            if (trainTargetPersistent == false)
            {
                _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
            }

            yield return new WaitForSecondsRealtime(0.5f);

            // Start stimulus and wait for the trial to end
            StartStimulusRun(false);
            yield return StartCoroutine(WaitForStimulusToComplete());
            yield return new WaitForSecondsRealtime(trainBufferTime);

            // If sham feedback is true, then show it
            if (shamFeedback)
            {
                _selectableSPOs[trainTarget].GetComponent<SPO>().Select();
            }

            // Turn off train target
            yield return new WaitForSecondsRealtime(0.5f);

            if (trainTargetPersistent == true)
            {
                _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
            }

            // Take a break
            yield return new WaitForSecondsRealtime(trainBreak);

            trainTarget = 99;

            Debug.Log("User Training Complete");

            blockOutGoingLSL = false;

            yield return null;
        }

        protected override IEnumerator OnStimulusRunBehavior()
        {
            numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());
            // numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            // UnityEngine.Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

            if (singleFlash)
            {
                //This may not be the right way to do this.
                Coroutine single_flashCR = Stop_Coroutines_Then_Start_New_Coroutine(ref _runStimulus, SingleFlashRoutine());
                yield return single_flashCR;
                //Old method
                //StopStartCoroutine(ref _runStimulus, SingleFlashRoutine());

            }

            if (contextAwareSingleFlash)
            {
                //This may not be the right way to do this.
                Coroutine single_context_flashCR = Stop_Coroutines_Then_Start_New_Coroutine(ref _runStimulus,  ContextAwareSingleFlashRoutine());
                yield return single_context_flashCR;
                //Old method
                //StopStartCoroutine(ref _runStimulus, ContextAwareSingleFlashRoutine());

            }

            if (multiFlash)
            {

                if (rowColumn)
                {
                    //StopStartCoroutine(ref _runStimulus, RowColFlashRoutine());
                    Coroutine rc_flashCR = Stop_Coroutines_Then_Start_New_Coroutine(ref _runStimulus, RowColFlashRoutine());
                    yield return rc_flashCR;
                }

                if (checkerboard)
                {
                    //StopStartCoroutine(ref _runStimulus, CheckerboardFlashRoutine());
                    Coroutine cb_flashCR = Stop_Coroutines_Then_Start_New_Coroutine(ref _runStimulus, CheckerboardFlashRoutine());
                    yield return cb_flashCR;
                }

                if(contextAwareMultiFlash)
                {
                    //StopStartCoroutine(ref _runStimulus, ContextAwareMultiFlashRoutine());
                    Coroutine context_multi_flashCR = Stop_Coroutines_Then_Start_New_Coroutine(ref _runStimulus, ContextAwareMultiFlashRoutine());
                    yield return context_multi_flashCR;
                }


            }

            //This is what is causing the issue with the "Trial Ends" marker being sent too early I think.
            StopStimulusRun();
            
            yield return null;
        }

        //TODO There is a bug where this is sending "Trial Ended" before the last flash is sent.
        private IEnumerator SingleFlashRoutine()
        {
            int totalFlashes = numFlashesPerObjectPerSelection * _selectableSPOs.Count;
            int[] stimOrder = ArrayUtilities.GenerateRNRA_FisherYates(totalFlashes, 0, _selectableSPOs.Count - 1);

            for (int i = 0; i < stimOrder.Length; i++)
            {
                int activeIndex = stimOrder[i];
                GameObject currentObject = _selectableSPOs[activeIndex]?.gameObject;

                // Turn on
                currentObject.GetComponent<SPO>().StartStimulus();

                // Send marker
                if (!blockOutGoingLSL)
                {
                    OutStream.PushSingleFlashP300Marker
                    (
                        SPOCount, activeIndex, trainTarget
                    );
                }

                // Wait
                yield return new WaitForSecondsRealtime(onTime);

                // Turn off
                currentObject.GetComponent<SPO>().StopStimulus();

                // Wait
                yield return new WaitForSecondsRealtime(offTime);
            }
        }
        
        private IEnumerator ContextAwareSingleFlashRoutine()
        {

            int totalFlashes = numFlashesPerObjectPerSelection;          

            //Need to send over not the order, but the specific unique object ID for selection/parsing to make sure we don't care where it is in a list.
            for (int jj = 0; jj < totalFlashes; jj++)
            {
                //Refresh the List of available SPO Objects. This will update the _validGOs list.
                PopulateObjectList();
                //Now get the Graph set for the TSP
                //Debug.Log("Getting the GraphBP For Context Aware Single Flash, updating each loop");
                int[] stimOrder = CalculateGraphTSP(_validGOs);

                for (int i = 0; i < stimOrder.Length; i++)
                {
                    int activeIndex = stimOrder[i];
                    SPO currentObject = _selectableSPOs[activeIndex]?.gameObject?.GetComponent<SPO>();

                    string markerString = "p300,s," + _selectableSPOs.Count.ToString();

                    if (trainTarget <= _selectableSPOs.Count)
                    {
                        markerString = markerString + "," + trainTarget.ToString();
                    }
                    else
                    {
                        markerString = markerString + "," + "-1";
                    }
                    Debug.LogWarning("MARKERS ARE BEING SENT FOR OBJECT IDS NOT OBJECT POSITIONS, SO THIS WILL NOT WORK WITH BESSY PYTHON JUST YET");
                    markerString = markerString + "," + currentObject.GetComponent<SPO>().ObjectID.ToString();

                    // Turn on
                    currentObject.StartStimulus();

                    // Send marker
                    if (!blockOutGoingLSL)
                    {
                        Debug.LogWarning("MARKERS ARE BEING SENT FOR OBJECT IDS NOT OBJECT POSITIONS, SO THIS WILL NOT WORK WITH BESSY PYTHON JUST YET");
                        OutStream.PushSingleFlashP300Marker
                        (
                            SPOCount, currentObject.ObjectID, trainTarget
                        );
                    }

                    // Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    // Turn off
                    currentObject.StopStimulus();

                    // Wait
                    yield return new WaitForSecondsRealtime(offTime);
                }
            }
        }

        
        private IEnumerator ContextAwareMultiFlashRoutine()
        {
            //Total number of flashes for each grouping of objects.
            int totalFlashes = numFlashesPerObjectPerSelection;
                      
            for (int jj = 0; jj < totalFlashes; jj++)
            {
                //Debug.Log("Getting the graph partition for the Context-Aware MultiFlash, updating each loop");
                var (subset1,subset2) = CalculateGraphPartition(_validGOs);

                //Turn the subsets into randomized matrices,
                int[,] randMat1 = SubsetToRandomMatrix(subset1);
                int[,] randMat2 = SubsetToRandomMatrix(subset2);
                
                //Flash through the rows of randMat1 first, then randMat2.
                //Off time is included in these coroutines.
                yield return StartCoroutine(FlashRowsSubsets(randMat1));
                yield return StartCoroutine(FlashRowsSubsets(randMat2));
                yield return StartCoroutine(FlashColsSubsets(randMat1));
                yield return StartCoroutine(FlashColsSubsets(randMat2));

                //Now shuffle!
            }


            yield return null;

        }

        private int[,] SubsetToRandomMatrix(int[] subset)
        {
            // Debug.Log("Original Subset" + string.Join(",",subset));
            int[] permutationArray = ArrayUtilities.GenerateRNRA_FisherYates(subset.Length,0,subset.Length-1);
            int[] subsetPermutated = new int[subset.Length];
            //Apply the permutation to the subset
            for (int i = 0; i < subset.Length; i++)
            {
                subsetPermutated[i] = subset[permutationArray[i]];
            }
            // Debug.Log("Shuffled Subset" + string.Join(",",subsetPermutated));
            var numRows = (int)Mathf.Floor(Mathf.Sqrt(subset.Length));
            var numCols = (int)Mathf.Ceil((float)subset.Length / (float)numRows);
            var newMatrix = new int[numRows, numCols];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    int index = i * numCols + j;
                    if (index < subset.Length)
                    {
                        newMatrix[i, j] = subsetPermutated[index];
                    }
                    else
                    {
                        newMatrix[i, j] = -100;
                    }

                }
            } 

            // Debug.Log("New Matrix:\n" + ArrayUtilities.FormatMatrix(newMatrix));
            return newMatrix;
        }

        private IEnumerator FlashRowsSubsets(int[,] subset1)
        {
            for (int i = 0; i < subset1.GetLength(0); i++)
            {
                List<int> objToFlash = new List<int>();

                for (int j = 0; j < subset1.GetLength(1); j++)
                {
                    if (subset1[i,j] != -100)
                    {
                        //Add the object to the list of objects to flash for marker writing
                        objToFlash.Add(subset1[i,j]);
                        _selectableSPOs[subset1[i,j]]?.StartStimulus();
                    }
                }
                //Write the marker for the objects to flash
                WriteMultiFlashMarker(objToFlash);
                yield return new WaitForSecondsRealtime(onTime);
                for (int j = 0; j < subset1.GetLength(1); j++)
                {
                    if (subset1[i,j] != -100)
                    {
                        _selectableSPOs[subset1[i,j]]?.StopStimulus();
                    }
                }
                yield return new WaitForSecondsRealtime(offTime);
            }
        }

        private IEnumerator FlashColsSubsets(int[,] subset1)
        {
            for (int i = 0; i < subset1.GetLength(1); i++)
            {
                //Create empty list to flash
                List<int> objToFlash = new List<int>();

                for (int j = 0; j < subset1.GetLength(0); j++)
                {
                    if (subset1[j,i] != -100)
                    {
                        //Add the object to the list of objects to flash for marker writing
                        objToFlash.Add(subset1[j,i]);
                        _selectableSPOs[subset1[j,i]]?.StartStimulus();
                    }
                }
                //Write the marker for the objects to flash
                WriteMultiFlashMarker(objToFlash);
                yield return new WaitForSecondsRealtime(onTime);
                for (int j = 0; j < subset1.GetLength(0); j++)
                {
                    if (subset1[j,i] != -100)
                    {
                        _selectableSPOs[subset1[j,i]]?.StopStimulus();
                    }
                }
                yield return new WaitForSecondsRealtime(offTime);
            }
        }

        private void WriteMultiFlashMarker(List<int> objToFlash)
        {
            if (blockOutGoingLSL == false)
            {
                OutStream.PushMultiFlashP300Marker
                (
                    SPOCount, objToFlash, trainTarget
                );
            }
        }

        private IEnumerator RowColFlashRoutine()
        {
            // // For multi flash selection, create virtual rows and columns
            // int numSelections = _selectableSPOs.Count;
            // int numColumns = (int)Math.Ceiling(Math.Sqrt((float)numSelections));
            // int numRows = (int)Math.Ceiling((float)numSelections / (float)numColumns);

            // int[,] rcMatrix = new int[numColumns, numRows];
            int[,] rcMatrix = Initialize2DMultiFlash();
            int numRows = rcMatrix.GetLength(0);
            int numColumns = rcMatrix.GetLength(1);
            int count = 0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (count <= _selectableSPOs.Count)
                        rcMatrix[i, j] = count;
                    //print(i.ToString() + j.ToString() + count.ToString());
                    count++;
                }
            }

            // Number of flashes per row/column
            int totalColumnFlashes = numFlashesPerObjectPerSelection * numColumns;
            int totalRowFlashes = numFlashesPerObjectPerSelection * numRows;

            // Create a random order to flash rows and columns
            int[] columnStimOrder = ArrayUtilities.GenerateRNRA_FisherYates(totalColumnFlashes, 0, numColumns-1);
            int[] rowStimOrder = ArrayUtilities.GenerateRNRA_FisherYates(totalRowFlashes, 0, numRows-1);

            for (int i = 0; i < totalColumnFlashes; i++)
            {
                // Turn on column 
                int columnIndex = columnStimOrder[i];
                List<int> flashingObjects = new();
                for (int n = 0; n < numRows; n++)
                {
                    int objectIndex = rcMatrix[n, columnIndex];
                    _selectableSPOs[objectIndex]?.StartStimulus();
                    flashingObjects.Add(objectIndex);
                }

                // Send marker
                WriteMultiFlashMarker(flashingObjects);

                //Wait
                yield return new WaitForSecondsRealtime(onTime);

                //Turn off column
                for (int n = 0; n < numRows; n++)
                {
                    _selectableSPOs[rcMatrix[n, columnIndex]]?.StopStimulus();
                }

                //Wait
                yield return new WaitForSecondsRealtime(offTime);

                // Flash row if available
                if (i <= totalRowFlashes)
                {
                    // Turn on row
                    int rowIndex = rowStimOrder[i];
                    flashingObjects.Clear();
                    for (int m = 0; m < numColumns; m++)
                    {
                        int objectIndex = rcMatrix[rowIndex, m];
                        //Turn on row
                        _selectableSPOs[objectIndex]?.StartStimulus();

                        //Add to marker
                        flashingObjects.Add(objectIndex);
                    }

                    //Send Marker
                    if (blockOutGoingLSL == false)
                    WriteMultiFlashMarker(flashingObjects);

                    //Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    //Turn off Row
                    for (int m = 0; m < numColumns; m++)
                    {
                        _selectableSPOs[rcMatrix[rowIndex, m]].StopStimulus();
                    }


                    //Wait
                    yield return new WaitForSecondsRealtime(offTime);
                }
            }
        }
        
        //TODO: Need to fix Checkerboard Flashing while I'm here
        private IEnumerator CheckerboardFlashRoutine()
        {
            // For multi flash selection, create virtual rows and columns
            // int numSelections = _selectableSPOs.Count;
            // int numColumns = (int)Math.Ceiling(Math.Sqrt((float)numSelections));
            // int numRows = (int)Math.Ceiling((float)numSelections / (float)numColumns);

            // int[,] rcMatrix = new int[numColumns, numRows];
            int[,] rcMatrix = Initialize2DMultiFlash();
            int numColumns = rcMatrix.GetLength(0);
            int numRows = rcMatrix.GetLength(1);
                                // get the size of the black/white matrices
            double maxBWsize = Math.Ceiling((numRows * numColumns) / 2f);

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

            Debug.Log("There are " + bwRows.ToString() + " rows and " + bwCols.ToString() +
                        " columns in the BW matrices");

            //This isn't actually shuffling between each target. Need to include it in the loop.
            Random rnd = new Random();
            int[] shuffledArray = Enumerable.Range(0, _selectableSPOs.Count).OrderBy(c => rnd.Next()).ToArray();

            // assign from CB to BW
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {

                // if there is an odd number of columns
                if (numColumns % 2 == 1)
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
                if (numColumns % 2 == 0)
                {
                    //assigned to black
                    int numR = shuffledArray[i] / numColumns;
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
            Debug.Log("blacks");
            LogArrayValues(blackList);
            Debug.Log("whites");
            LogArrayValues(whiteList);

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

                    LogArrayValues(objectsToFlash);

                    List<int> flashingObjects = new();
                    // Turn on objects to flash
                    for (int fi = 0; fi < bwCols; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            int objectIndex = objectsToFlash[fi];
                            //Turn on row
                            _selectableSPOs[objectIndex].StartStimulus();

                            //Add to marker
                            flashingObjects.Add(objectIndex);
                        }
                    }

                    //Send Marker
                    WriteMultiFlashMarker(flashingObjects);

                    //Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    //Turn off objects to flash
                    for (int fi = 0; fi < bwCols; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            //Turn on row
                            _selectableSPOs[objectsToFlash[fi]].StopStimulus();
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

                    LogArrayValues(objectsToFlash);

                    List<int> flashingObjects = new();
                    // Turn on objects to flash
                    for (int fi = 0; fi < bwCols; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            int objectIndex = objectsToFlash[fi];
                            //Turn on row
                            _selectableSPOs[objectIndex].StartStimulus();

                            //Add to marker
                            flashingObjects.Add(objectIndex);
                        }
                    }

                    //Send Marker
                    WriteMultiFlashMarker(flashingObjects);

                    //Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    //Turn off objects to flash
                    for (int fi = 0; fi < bwCols; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            //Turn on row
                            _selectableSPOs[objectsToFlash[fi]].StopStimulus();
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

                    LogArrayValues(objectsToFlash);

                    List<int> flashingObjects = new();
                    // Turn on objects to flash
                    for (int fi = 0; fi < bwRows; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            int objectIndex = objectsToFlash[fi];
                            //Turn on row
                            _selectableSPOs[objectIndex].StartStimulus();

                            //Add to marker
                            flashingObjects.Add(objectIndex);
                        }
                    }

                    //Send Marker
                    WriteMultiFlashMarker(flashingObjects);

                    //Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    //Turn off objects to flash
                    for (int fi = 0; fi < bwRows; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            //Turn on row
                            _selectableSPOs[objectsToFlash[fi]].StopStimulus();
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

                    LogArrayValues(objectsToFlash);

                    
                    List<int> flashingObjects = new();
                    // Turn on objects to flash
                    for (int fi = 0; fi < bwRows; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            int objectID = objectsToFlash[fi];
                            //Turn on row
                            _selectableSPOs[objectID].StartStimulus();

                            //Add to marker
                            flashingObjects.Add(objectID);
                        }
                    }

                    //Send Marker
                    WriteMultiFlashMarker(flashingObjects);

                    //Wait
                    yield return new WaitForSecondsRealtime(onTime);

                    //Turn off objects to flash
                    for (int fi = 0; fi < bwRows; fi++)
                    {
                        if (objectsToFlash[fi] != 99)
                        {
                            //Turn on row
                            _selectableSPOs[objectsToFlash[fi]].StopStimulus();
                        }
                    }

                    //Wait
                    yield return new WaitForSecondsRealtime(offTime);
                }
            }

        }
        
        private int[,] Initialize2DMultiFlash()
        {
                // For multi flash selection, create virtual rows and columns
                //There is some bug with setting this for the row column flashing, but it works for checkerboard. 
                // Commenting it out for now.
                // int numColumns = (int)Math.Ceiling(Math.Sqrt((float)numSelections));
                // int numRows = (int)Math.Ceiling((float)numSelections / (float)numColumns);
                int numColumns = numFlashColumns;
                int numRows = numFlashRows;

                int[,] rcMatrix = new int[numRows, numColumns];
                return rcMatrix;
        }

        /// <summary>
        /// Populate the <see cref="SelectableSPOs"/> using a particular method.
        /// This extends the typical BCI Controller Behavior to enable "context
        /// aware" selection of SPOs.
        /// </summary>
        /// <param name="populationMethod">Method of population to use</param>
        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.GraphBP)
        {
            switch (populationMethod)
            {
                case SpoPopulationMethod.Predefined:
                    //Use the current contents of object list
                    break;
                case SpoPopulationMethod.Children:
                    Debug.LogWarning("Populating by children is not yet implemented");
                    break;
                default:
                case SpoPopulationMethod.Tag:
                    _selectableSPOs.Clear();
                    var taggedGOs = GameObject.FindGameObjectsWithTag(SPOTag);
                    foreach (var taggedGO in taggedGOs)
                    {
                        if (!taggedGO.TryGetComponent<SPO>(out var spo) || !spo.Selectable)
                        {
                            continue;
                        }

                        // Check if the object has a unique ObjectID, 
                        // if not assign it a unique ID
                        if (taggedGO.GetComponent<SPO>().ObjectID == -100)
                        {
                            taggedGO.GetComponent<SPO>().ObjectID = __uniqueP300ID;
                            __uniqueP300ID++;
                        }

                        _selectableSPOs.Add(spo);
                        _objectIDtoSPODict.Add(taggedGO.GetComponent<SPO>().ObjectID, spo);
                        //Print out the Dictionary Pairs
                        Debug.Log("ObjectID: " + taggedGO.GetComponent<SPO>().ObjectID + " SPO: " + spo);
                        spo.SelectablePoolIndex = _selectableSPOs.Count - 1;
                    }
                    break;
                case SpoPopulationMethod.GraphBP:
                    _selectableSPOs.Clear();
                    
                    //First, get all game objects in the world visible by the camera, including the UI.
                    // This will populate the _selectableSPOs list
                    _validGOs = GetGameSPOsInCameraView();                   
                    
                    break;
            }
        }

        //This is the non-multi-camera version of the function
        public List<GameObject> GetGameSPOsInCameraView()
        {
            Camera mainCamera = Camera.main;
            var taggedGOs = GameObject.FindGameObjectsWithTag(SPOTag);
            List<GameObject> visibleGOs = new List<GameObject>();
            List<GameObject> uiGOs = new List<GameObject>();

            foreach (var obj in taggedGOs)
            {
                // Handle the UI objects separately.
                //Right now, can't get the CanvasRenderer to understand when objects in the UI might be off screen. It's a niche problem, but worth noting.
                if (obj.layer == 5)
                {
                    if (!obj.TryGetComponent<SPO>(out var uiSpo) || !uiSpo.Selectable || !obj.GetComponent<CanvasRenderer>().IsVisibleFromCanvas(mainCamera))
                    {
                        continue;
                    }
                    // Check if the object has a unique ObjectID, 
                    // if not assign it a unique ID
                    if (obj.GetComponent<SPO>().ObjectID == -100)
                    {
                        obj.GetComponent<SPO>().ObjectID = __uniqueP300ID;
                        __uniqueP300ID++;
                    }
                    uiGOs.Add(obj);
                    _selectableSPOs.Add(uiSpo);
                    if (!_objectIDtoSPODict.ContainsKey(obj.GetComponent<SPO>().ObjectID))
                    {
                        _objectIDtoSPODict.Add(obj.GetComponent<SPO>().ObjectID, uiSpo);
                    }
                    uiSpo.SelectablePoolIndex = _selectableSPOs.Count - 1;
                    continue;
                }

                if (!obj.TryGetComponent<SPO>(out var spo) || !spo.Selectable || !obj.GetComponent<Renderer>().IsVisibleFrom(mainCamera))
                {
                    continue;
                }
                // Check if the object has a unique ObjectID, 
                // if not assign it a unique ID
                if (obj.GetComponent<SPO>().ObjectID == -100)
                {
                    obj.GetComponent<SPO>().ObjectID = __uniqueP300ID;
                    __uniqueP300ID++;
                }
                visibleGOs.Add(obj);
                _selectableSPOs.Add(spo);
                if (!_objectIDtoSPODict.ContainsKey(obj.GetComponent<SPO>().ObjectID))
                {
                    _objectIDtoSPODict.Add(obj.GetComponent<SPO>().ObjectID, spo);
                }
                spo.SelectablePoolIndex = _selectableSPOs.Count - 1;
            }
            var allValidGOs = uiGOs.Concat(visibleGOs).ToList();
            return allValidGOs;
        }

        public int[] CalculateGraphTSP(List<GameObject> nodes, bool debugPrint = false)
        {
            //Get the world points of each item with respect to the camera
            // var cameraTranfsorm = Camera.main.transform;
            List<Vector2> correctedNodePositions = CalculateOffsetFromCamera(nodes, Camera.main);
            int numNodes = nodes.Count;
            float[,] objectWeights = new float[numNodes, numNodes];
        
            foreach (var node in nodes)
            {
                //use Vector3.Angle to get the angle between every object in the scene. Store this as weights in a graph
                //This is a symmetric matrix, so we only need to calculate the upper triangle
                for (int i = 0; i < numNodes; i++)
                {
                    if (i == nodes.IndexOf(node))
                    {
                        objectWeights[nodes.IndexOf(node), i] = 0;
                    }
                    else
                    {
                        objectWeights[nodes.IndexOf(node), i] = Vector3.Angle(correctedNodePositions[nodes.IndexOf(node)], correctedNodePositions[i]);
                    }
                }
            }

            if (debugPrint)
            {
                //Print the weights in the upper triangle matrix
                for (int i = 0; i < numNodes; i++)
                {
                    for (int j = 0; j < numNodes; j++)
                    {
                        if (j >= i && objectWeights[i, j] != 0)
                        {
                            Debug.Log($"Angle (weight) between {nodes[i].name} and {nodes[j].name}: {objectWeights[i, j]}");
                        }
                    }
                }
            }


            GraphUtilities tsp = new GraphUtilities();
            
            var startNode = UnityEngine.Random.Range(0, numNodes);
            //Make sure the start node and last node of the tour are not the same
            if(startNode == lastTourEndNode)
            {
                //chose a different start node
                startNode = (startNode+1) % numNodes;
            }
          
            var tour = tsp.SolveModifiedTSP(objectWeights, startNode);
            lastTourEndNode = tour[tour.Count - 1];

            if(debugPrint)
            {
                Debug.Log("The start node is " + startNode.ToString());
                            //Print out the tour
                for (int i = 0; i < tour.Count; i++)
                {
                    Debug.Log("The tour is " + tour[i].ToString());
                }
                Debug.Log("Tour length is: " + tsp.CalculateTourLength(tour));
            }

            int[] tourArray = tour.ToArray();
            return tourArray;
  

            //Use the graph utilities to do the modified TSP.      

        }

        public (int[] subset1, int[] subset2) CalculateGraphPartition(List<GameObject> nodes)
        {
            // Get 2D screen positions
            List<Vector2> screenPositions = CalculateOffsetFromCamera(nodes, Camera.main);
            int numNodes = nodes.Count;
            float[,] objectWeights = new float[numNodes, numNodes];

            foreach (var node in nodes)
            {
                int i = nodes.IndexOf(node);
                for (int j = 0; j < numNodes; j++)
                {
                    if (i == j)
                    {
                        objectWeights[i, j] = 0;
                    }
                    else
                    {
                        // Calculate 2D screen-space distance
                        float distance = Vector2.Distance(
                            screenPositions[i], 
                            screenPositions[j]
                        );
                        // Convert distance to weight (inverse relationship)
                        objectWeights[i, j] = 1.0f / (distance + 1.0f);
                    }
                }
            }

            var lpPart = new GraphUtilities();
            return lpPart.LaplaceGP(objectWeights);
        }

        public List<Vector2> CalculateOffsetFromCamera(List<GameObject> goList, Camera myCamera)
        {
            List<Vector2> screenPositions = new();
            foreach (var obj in goList)
            {
                if (obj.layer == 5 || obj.TryGetComponent<RectTransform>(out var rectT))
                {
                    // UI Elements - get screen position directly from RectTransform
                    RectTransform tempRT = obj.GetComponent<RectTransform>();
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(
                        tempRT,
                        tempRT.position,
                        myCamera,
                        out Vector3 screenPos
                    );
                    screenPositions.Add(new Vector2(screenPos.x, screenPos.y));
                }
                else
                {
                    // 3D Objects - project to screen space
                    Vector3 screenPos = myCamera.WorldToScreenPoint(obj.transform.position);
                    screenPositions.Add(screenPos);
                }
            }
            return screenPositions;
        }
        
        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Do nothing, markers are are temporally bound to stimulus and are therefore sent from stimulus coroutine
            yield return null;
        }

        // Turn the stimulus on
        public override void StartStimulusRun(bool sendConstantMarkers = true)
        {
            StimulusRunning = true;
            
            StimulusRunning = true;
            LastSelectedSPO = null;
            
            // Send the marker to start
            if (blockOutGoingLSL == false)
            {
                OutStream.PushTrialStartedMarker();
            }

            StartReceivingMarkers();
            PopulateObjectList();
            StopStartCoroutine(ref _runStimulus, RunStimulus());

            // Not required for P300
            if (sendConstantMarkers)
            {
                StopStartCoroutine(ref _sendMarkers, SendMarkers(trainTarget));
            }
        }

        public override void StopStimulusRun()
        {
            // End thhe stimulus Coroutine
            StimulusRunning = false;
            
            // Send the marker to end
            if (blockOutGoingLSL == false)
            {
                OutStream.PushTrialEndsMarker();
            }
        }



        #region Experimental Calculations
        public override void SelectSPO(int objectID, bool stopStimulusRun = false)
        {
            var objectCount = _selectableSPOs.Count;
            if (objectCount == 0)
            {
                Debug.Log("No Objects to select");
                return;
            }

            if (_objectIDtoSPODict.ContainsKey(objectID))
            {
                var spo = _objectIDtoSPODict[objectID];
                if (spo == null)
                {
                    Debug.LogWarning("SPO is now null and can't be selected");
                    return;
                }

                spo.Select();
                LastSelectedSPO = spo;
                Debug.Log($"SPO '{spo.gameObject.name}' selected.");

                if (stopStimulusRun)
                {
                    StopStimulusRun();
                }
            }
            else
            {
                Debug.LogWarning($"Invalid Selection. Must be in the objectID dictionary");
                return;
            }
        }

        #endregion

    }
}
