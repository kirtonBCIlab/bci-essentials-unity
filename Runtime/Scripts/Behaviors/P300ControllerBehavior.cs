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
        }

        //TODO There is a bug where this is sending "Trial Ended" before the last flash is sent.
        private IEnumerator RunSingleFlashRoutine()
        {
            int totalFlashes = numFlashesPerObjectPerSelection * _selectableSPOs.Count;
            int[] stimOrder = ArrayUtilities.GenerateRNRA_FisherYates(totalFlashes, 0, _selectableSPOs.Count - 1);

            foreach (int stimIndex in stimOrder)
            {
                yield return RunSingleFlash(stimIndex);
            }
        }
        
        private IEnumerator RunContextAwareSingleFlashRoutine()
        {
            int totalFlashes = numFlashesPerObjectPerSelection;

            //Need to send over not the order, but the specific unique object ID for selection/parsing to make sure we don't care where it is in a list.
            for (int jj = 0; jj < totalFlashes; jj++)
            {
                List<SPO> visibleSPOs = GetCameraVisibleSPOs();
                var visibleGameObjects = visibleSPOs.SelectGameObjects();
                int[] stimOrder = CalculateGraphTSP(visibleGameObjects, ref lastTourEndNode);

                foreach (int stimIndex in stimOrder)
                {
                    yield return RunSingleFlash(stimIndex, visibleSPOs);
                }
            }
        }

        protected IEnumerator RunSingleFlash(int activeIndex)
        => RunSingleFlash(activeIndex, _selectableSPOs);
        protected IEnumerator RunSingleFlash
        (
            int activeIndex, List<SPO> stimulusObjects
        )
        {
            SPO flashingObject = stimulusObjects[activeIndex];
            flashingObject.StartStimulus();
            SendSingleFlashMarker(activeIndex, stimulusObjects.Count);
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
                List<SPO> visibleSPOs = GetCameraVisibleSPOs();
                var visibleGameObjects = visibleSPOs.SelectGameObjects();
                var (subset1,subset2) = CalculateGraphPartition(visibleGameObjects);

                IEnumerator RunMultiFlashOnVisibleObjects(int[] flashingIndices)
                => RunMultiFlash(flashingIndices, visibleSPOs);

                //Turn the subsets into randomized matrices,
                int[,] randMat1 = SubsetToRandomMatrix(subset1);
                int[,] randMat2 = SubsetToRandomMatrix(subset2);
                
                //Flash through the rows of randMat1 first, then randMat2.
                //Off time is included in these coroutines.
                yield return randMat1.RunForEachRow(RunMultiFlashOnVisibleObjects);
                yield return randMat2.RunForEachRow(RunMultiFlashOnVisibleObjects);
                yield return randMat1.RunForEachColumn(RunMultiFlashOnVisibleObjects);
                yield return randMat2.RunForEachColumn(RunMultiFlashOnVisibleObjects);

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


        private IEnumerator RunCheckerboardFlashRoutine()
        {
            int[,] rcMatrix = InitializeFlashingIndexGrid();
            BlackWhiteMatrixFactory bwMatrixFactory = new(rcMatrix);

            int[,] blackMatrix, whiteMatrix;
            (blackMatrix, whiteMatrix) = bwMatrixFactory.CreateShuffledMatrices();

            for (int f = 0; f < numFlashesPerObjectPerSelection; f++)
            {
                yield return blackMatrix.RunForEachRow(RunMultiFlash);
                yield return whiteMatrix.RunForEachRow(RunMultiFlash);
                yield return blackMatrix.RunForEachColumn(RunMultiFlash);
                yield return whiteMatrix.RunForEachColumn(RunMultiFlash);

                (blackMatrix, whiteMatrix) = bwMatrixFactory.CreateShuffledMatrices();
            }
        }
        
        private int[,] InitializeFlashingIndexGrid()
        {
            // For multi flash selection, create virtual rows and columns
            // Possible to do programmatically if certain conditions are met
            // such as checking for use of an SPOGridFactory
            return new int[numFlashRows, numFlashColumns];
        }

        protected IEnumerator RunMultiFlash(int[] objectsToFlash)
        => RunMultiFlash(objectsToFlash, _selectableSPOs);
        protected IEnumerator RunMultiFlash
        (
            int[] objectsToFlash, List<SPO> stimulusObjects
        )
        {
            int objectCount = stimulusObjects.Count;
            objectsToFlash = objectsToFlash.Where
            (x => x >= 0 && x < objectCount).ToArray();

            foreach (int i in objectsToFlash)
                stimulusObjects[i].StartStimulus();

            SendMultiFlashMarker(objectsToFlash, objectCount);
            yield return new WaitForSecondsRealtime(onTime);

            foreach(int i in objectsToFlash)
                stimulusObjects[i].StopStimulus();
            yield return new WaitForSecondsRealtime(offTime);
        }

        
        public List<SPO> GetCameraVisibleSPOs()
        {
            Camera mainCamera = Camera.main;
            List<SPO> visibleSPOs = new();

            foreach (SPO spo in _selectableSPOs)
            {
                if (
                    (spo.TryGetComponent(out CanvasRenderer canvasRenderer)
                    && canvasRenderer.IsVisibleFromCanvas(mainCamera))
                    ||
                    (spo.TryGetComponent(out Renderer renderer)
                    && renderer.IsVisibleFrom(mainCamera))
                )
                visibleSPOs.Add(spo);
            }
            return visibleSPOs;
        }


        private void SendSingleFlashMarker
        (
            int flashedObjectIndex, int objectCount
        )
        {
            if (MarkerWriter != null && !blockOutGoingLSL)
            {
                MarkerWriter.PushSingleFlashP300Marker(
                    objectCount, flashedObjectIndex, trainTarget
                );
            }
        }

        private void SendMultiFlashMarker
        (
            IEnumerable<int> flashedObjectIndices, int objectCount
        )
        {
            if (MarkerWriter != null && !blockOutGoingLSL)
            {
                MarkerWriter.PushMultiFlashP300Marker
                (
                    objectCount, flashedObjectIndices, trainTarget
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
    }
}
