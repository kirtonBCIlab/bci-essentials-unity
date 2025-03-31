using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Random = System.Random;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using BCIEssentials.StimulusObjects;


namespace BCIEssentials.ControllerBehaviors
{
    using static ContextAwareUtilities;

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


        protected override IEnumerator RunUserTrainingRoutine()
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

        protected override IEnumerator RunStimulusRoutine()
        {
            numFlashesPerObjectPerSelection = randNumFlashes.Next(numFlashesLowerLimit, numFlashesUpperLimit);
            Debug.Log("Number of flashes is " + numFlashesPerObjectPerSelection.ToString());

            yield return flashPlurality switch
            {
                FlashPlurality.Single=> singleFlashPattern switch
                {
                    SingleFlashPattern.Random => RunSingleFlashRoutine(),
                    SingleFlashPattern.ContextAware => RunContextAwareSingleFlashRoutine(),
                    _  => null
                },
                FlashPlurality.Multiple => multiFlashPattern switch
                {
                    MultiFlashPattern.RowColumn => RunRowColFlashRoutine(),
                    MultiFlashPattern.Checkerboard => RunCheckerboardFlashRoutine(),
                    MultiFlashPattern.ContextAware => RunContextAwareMultiFlashRoutine(),
                    _ => null
                },
                _ => null
            };
            
            StopStimulusRun();
        }

        //TODO There is a bug where this is sending "Trial Ended" before the last flash is sent.
        private IEnumerator RunSingleFlashRoutine()
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
        
        private IEnumerator RunContextAwareSingleFlashRoutine()
        {
            int totalFlashes = numFlashesPerObjectPerSelection;          

            //Need to send over not the order, but the specific unique object ID for selection/parsing to make sure we don't care where it is in a list.
            for (int jj = 0; jj < totalFlashes; jj++)
            {
                //Refresh the List of available SPO Objects. This will update the _validGOs list.
                PopulateObjectList();
                //Now get the Graph set for the TSP
                int[] stimOrder = CalculateGraphTSP(_validGOs, ref lastTourEndNode);

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
            SendSingleFlashMarker(markerId);
            yield return new WaitForSecondsRealtime(onTime);

            flashingObject.StopStimulus();
            yield return new WaitForSecondsRealtime(offTime);
        }

        
        private IEnumerator RunContextAwareMultiFlashRoutine()
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


        private IEnumerator RunRowColFlashRoutine()
        {
            int[,] rcMatrix = InitializeFlashingIndexGrid();
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
        private IEnumerator RunCheckerboardFlashRoutine()
        {
            int[,] rcMatrix = InitializeFlashingIndexGrid();
            BlackWhiteMatrixFactory bwMatrixFactory = new(rcMatrix);
            
            (int[,] blackMat, int[,] whiteMat)
            = bwMatrixFactory.CreateShuffledMatrices();

            // for flash count
            for (int f = 0; f < numFlashesPerObjectPerSelection; f++)
            {
                yield return blackMat.RunForEachRow(RunMultiFlash);
                yield return whiteMat.RunForEachRow(RunMultiFlash);
                yield return blackMat.RunForEachColumn(RunMultiFlash);
                yield return whiteMat.RunForEachColumn(RunMultiFlash);
            }
        }
        
        private int[,] InitializeFlashingIndexGrid()
        {
            // For multi flash selection, create virtual rows and columns
            // Possible to do programmatically if certain conditions are met
            // such as checking for use of an SPOGridFactory
            return new int[numFlashRows, numFlashColumns];
        }

        private IEnumerator RunMultiFlash(int[] objectsToFlash)
        {
            objectsToFlash = objectsToFlash.Where
            (x => x >= 0 && x < SPOCount).ToArray();

            foreach (int i in objectsToFlash)
                _selectableSPOs[i].StartStimulus();

            SendMultiFlashMarker(objectsToFlash);
            yield return new WaitForSecondsRealtime(onTime);

            foreach(int i in objectsToFlash)
                _selectableSPOs[i].StopStimulus();
            yield return new WaitForSecondsRealtime(offTime);
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


        private void SendSingleFlashMarker(int objectIndex)
        {
            if (MarkerWriter != null && !blockOutGoingLSL)
            {
                MarkerWriter.PushSingleFlashP300Marker(
                    SPOCount, objectIndex, trainTarget
                );
            }
        }

        private void SendMultiFlashMarker(IEnumerable<int> flashedObjects)
        {
            if (MarkerWriter != null && !blockOutGoingLSL)
            {
                MarkerWriter.PushMultiFlashP300Marker
                (
                    SPOCount, flashedObjects, trainTarget
                );
            }
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
