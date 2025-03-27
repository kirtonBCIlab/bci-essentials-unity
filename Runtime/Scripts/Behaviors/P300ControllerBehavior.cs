using BCIEssentials.StimulusObjects;
using System.Collections;
using UnityEngine;
using System;
using System.Linq;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using Random = System.Random;
using System.Collections.Generic;
using UnityEditor.EditorTools;

namespace BCIEssentials.ControllerBehaviors
{
    public class P300ControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.P300;
        public enum FlashPlurality {Single, Multiple}
        public enum SingleFlashPattern {Random, ContextAware}
        public enum MultiFlashPattern {RowColumn, Checkerboard, ContextAware}


        [StartFoldoutGroup("P300 Pattern Flashing Properties")]
        public int numFlashesLowerLimit = 9;
        public int numFlashesUpperLimit = 12;
        public Random randNumFlashes = new Random();
        private int numFlashesPerObjectPerSelection = 3;

        [Tooltip("[sec]")]
        public float onTime = 0.1f;
        [Tooltip("[sec]")]
        public float offTime = 0.075f;


        [StartFoldoutGroup("Stimulus Flash Paradigms")]
        [Tooltip("Whether to flash multiple SPOs at a time or just one")]
        public FlashPlurality flashPlurality;

        [ShowIf("flashPlurality", (int)FlashPlurality.Single)]
        [Tooltip("Whether to enable context-aware SPO single flashing")]
        public SingleFlashPattern singleFlashPattern;

        [ShowIf("flashPlurality", (int)FlashPlurality.Multiple)]
        [Tooltip("The pattern in which to flash objects")]
        public MultiFlashPattern multiFlashPattern;


        [Header("Row/Column & Checkerboard Properties")]
        [ShowIf("flashPlurality", (int)FlashPlurality.Multiple)]
        [ShowIf("multiFlashPattern", (int)MultiFlashPattern.RowColumn, (int)MultiFlashPattern.Checkerboard)]
        [Tooltip("Number of rows in multi-flash RowColumn or Checkerboard")]
        public int numFlashRows = 5;
        [ShowIf("flashPlurality", (int)FlashPlurality.Multiple)]
        [ShowIf("multiFlashPattern", (int)MultiFlashPattern.RowColumn, (int)MultiFlashPattern.Checkerboard)]
        [Tooltip("Number of columns in the multi-flash RowColumn or Checkerboard")]
        public int numFlashColumns = 6;


        private bool blockOutGoingLSL = false;

        //I have updated the starting __uniqueP300ID to 0, as it was causing issues with the LSL markers at 1.
        private int __uniqueP300ID = 0;

        private List<GameObject> _validGOs = new List<GameObject>();
        private int lastTourEndNode = -100;


        protected override IEnumerator WhileDoUserTraining()
        {
            blockOutGoingLSL = true;

            PopulateObjectList();

            // Get a random training target
            trainTarget = randNumFlashes.Next(0, _selectableSPOs.Count - 1);
            Debug.Log($"Running User Training on option {trainTarget}");

            SPO targetObject = _selectableSPOs[trainTarget];
            yield return RunTrainingRound(targetObject);

            Debug.Log("User Training Complete");

            blockOutGoingLSL = false;
        }

        protected override IEnumerator OnStimulusRunBehavior()
        {
            numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

            yield return flashPlurality switch
            {
                FlashPlurality.Single=> singleFlashPattern switch
                {
                    SingleFlashPattern.Random => SingleFlashRoutine(),
                    SingleFlashPattern.ContextAware => ContextAwareSingleFlashRoutine(),
                    _  => null
                },
                FlashPlurality.Multiple => multiFlashPattern switch
                {
                    MultiFlashPattern.RowColumn => RowColFlashRoutine(),
                    MultiFlashPattern.Checkerboard => CheckerboardFlashRoutine(),
                    MultiFlashPattern.ContextAware => ContextAwareMultiFlashRoutine(),
                    _ => null
                },
                _ => null
            };
            
            StopStimulusRun();
        }

        //TODO There is a bug where this is sending "Trial Ended" before the last flash is sent.
        private IEnumerator SingleFlashRoutine()
        {
            int totalFlashes = numFlashesPerObjectPerSelection * _selectableSPOs.Count;
            int[] stimOrder = ArrayUtilities.GenerateRNRA_FisherYates(totalFlashes, 0, _selectableSPOs.Count - 1);

            for (int i = 0; i < stimOrder.Length; i++)
            {
                int activeIndex = stimOrder[i];
                SPO flashingObject = _selectableSPOs[activeIndex];
                yield return RunSingleFlash(flashingObject, activeIndex);
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
                int[] stimOrder = CalculateGraphTSP(_validGOs);

                for (int i = 0; i < stimOrder.Length; i++)
                {
                    int activeIndex = stimOrder[i];
                    SPO activeObject = _selectableSPOs[activeIndex];
                    Debug.LogWarning("MARKERS ARE BEING SENT FOR OBJECT IDS NOT OBJECT POSITIONS, SO THIS WILL NOT WORK WITH BESSY PYTHON JUST YET");
                    yield return RunSingleFlash(activeObject, activeObject.ObjectID);
                }
            }
        }

        private IEnumerator RunSingleFlash(SPO flashingObject, int markerId)
        {
            flashingObject.StartStimulus();
            WriteSingleFlashMarker(markerId);
            yield return new WaitForSecondsRealtime(onTime);

            flashingObject.StopStimulus();
            yield return new WaitForSecondsRealtime(offTime);
        }

        
        private IEnumerator ContextAwareMultiFlashRoutine()
        {
            //Total number of flashes for each grouping of objects.
            int totalFlashes = numFlashesPerObjectPerSelection;
                      
            for (int jj = 0; jj < totalFlashes; jj++)
            {
                PopulateObjectList();
                //Debug.Log("Getting the graph partition for the Context-Aware MultiFlash, updating each loop");
                var (subset1,subset2) = CalculateGraphPartition(_validGOs);

                //Turn the subsets into randomized matrices,
                int[,] randMat1 = SubsetToRandomMatrix(subset1);
                int[,] randMat2 = SubsetToRandomMatrix(subset2);
                
                //Flash through the rows of randMat1 first, then randMat2.
                //Off time is included in these coroutines.
                yield return randMat1.RunForEachRow(RunMultiFlash);
                yield return randMat2.RunForEachRow(RunMultiFlash);
                yield return randMat1.RunForEachColumn(RunMultiFlash);
                yield return randMat2.RunForEachColumn(RunMultiFlash);

                //Now shuffle!
            }
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


        private IEnumerator RowColFlashRoutine()
        {
            int[,] rcMatrix = Initialize2DMultiFlash();
            int numRows = rcMatrix.GetLength(0);
            int numColumns = rcMatrix.GetLength(1);
            int count = 0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (count <= _selectableSPOs.Count)
                        rcMatrix[i, j] = count++;
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
                // Flash column 
                int columnIndex = columnStimOrder[i];
                int[] column = rcMatrix.GetColumn(columnIndex);
                yield return RunMultiFlash(column);

                // Flash row if available
                if (i <= totalRowFlashes)
                {
                    int rowIndex = rowStimOrder[i];
                    int[] row = rcMatrix.GetRow(rowIndex);
                    yield return RunMultiFlash(row);
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

            // for flash count
            for (int f = 0; f < numFlashesPerObjectPerSelection; f++)
            {
                yield return blackMat.RunForEachRow(RunMultiFlash);
                yield return whiteMat.RunForEachRow(RunMultiFlash);
                yield return blackMat.RunForEachColumn(RunMultiFlash);
                yield return whiteMat.RunForEachColumn(RunMultiFlash);
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

        private IEnumerator RunMultiFlash(int[] objectsToFlash)
        {
            objectsToFlash = objectsToFlash.Where
            (x => x >= 0 && x < SPOCount).ToArray();

            foreach (int i in objectsToFlash)
                _selectableSPOs[i].StartStimulus();

            WriteMultiFlashMarker(objectsToFlash);
            yield return new WaitForSecondsRealtime(onTime);

            foreach(int i in objectsToFlash)
                _selectableSPOs[i].StopStimulus();
            yield return new WaitForSecondsRealtime(offTime);
        }


        private void WriteSingleFlashMarker(int objectIndex)
        {
            if (OutStream != null && !blockOutGoingLSL)
            {
                OutStream.PushSingleFlashP300Marker(
                    SPOCount, objectIndex, trainTarget
                );
            }
        }

        private void WriteMultiFlashMarker(IEnumerable<int> flashedObjects)
        {
            if (OutStream != null && !blockOutGoingLSL)
            {
                OutStream.PushMultiFlashP300Marker
                (
                    SPOCount, flashedObjects, trainTarget
                );
            }
        }


        /// <summary>
        /// Populate the <see cref="SelectableSPOs"/> using a particular method.
        /// This extends the typical BCI Controller Behavior to enable "context
        /// aware" selection of SPOs.
        /// </summary>
        /// <param name="populationMethod">Method of population to use</param>
        public override void PopulateObjectList
        (SpoPopulationMethod populationMethod = SpoPopulationMethod.GraphBP)
        {
            switch (populationMethod)
            {
                case SpoPopulationMethod.GraphBP:
                    //First, get all game objects in the world
                    //visible by the camera, including the UI.
                    _validGOs = GetSPOGameObjectsInCameraViewByTag();
                    _selectableSPOs = _validGOs.Select(
                        validGO => validGO.GetComponent<SPO>()
                    ).ToList();

                    AssignIDsToSelectableSPOs(ref __uniqueP300ID);
                    AppendSelectableSPOsToObjectIDDictionary();
                    break;
                case SpoPopulationMethod.Tag:
                    _selectableSPOs = GetSelectableSPOsByTag();
                    AssignIDsToSelectableSPOs(ref __uniqueP300ID);
                    AppendSelectableSPOsToObjectIDDictionary();
                    break;
                default:
                    base.PopulateObjectList(populationMethod);
                    break;
            }
        }

        //This is the non-multi-camera version of the function
        public List<GameObject> GetSPOGameObjectsInCameraViewByTag()
        {
            Camera mainCamera = Camera.main;
            List<GameObject> visibleGOs = new List<GameObject>();

            foreach (SPO spo in GetSelectableSPOsByTag())
            {
                if (
                    (spo.TryGetComponent(out CanvasRenderer canvasRenderer)
                    && canvasRenderer.IsVisibleFromCanvas(mainCamera))
                    ||
                    (spo.TryGetComponent(out Renderer renderer)
                    && renderer.IsVisibleFrom(mainCamera))
                )
                visibleGOs.Add(spo.gameObject);
            }
            return visibleGOs;
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

        protected override void SendTrialStartedMarker()
        {
            if (!blockOutGoingLSL) base.SendTrialStartedMarker();
        }

        protected override void SendTrialEndsMarker()
        {
            if (!blockOutGoingLSL) base.SendTrialEndsMarker();
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
