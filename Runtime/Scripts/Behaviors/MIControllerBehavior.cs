using System.Collections;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.ControllerBehaviors
{
    public class MIControllerBehavior : WindowedControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.MI;

        [Header("Iterative Training")]
        [SerializeField, AppendToFoldoutGroup("Training Properties")]
        [Tooltip("How many selections to make before creating the classifier")]
        protected int numSelectionsBeforeTraining = 3;
        [SerializeField, AppendToFoldoutGroup("Training Properties")]
        [Tooltip("How many selections to make before updating the classifier")]
        protected int numSelectionsBetweenTraining = 3;

        protected int selectionCounter = 0;
        protected int updateCounter = 0;


        public override void MakeSelection(int classIndex, bool stopStimulusRun = false)
        {
            base.MakeSelection(classIndex, stopStimulusRun);
            Debug.LogWarning(
                "You should override this method in an inherited class " +
                "as object selection is neither deterministic by index " +
                "between trials nor relevant to the paradigm."
            );
        }

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);

            // Warn about the number of objects to be selected from, if greater than 2
            if (_selectableSPOs.Count > 2)
            {
                Debug.LogWarning("Warning: Selecting between more than 2 objects!");
            }
        }

        protected override IEnumerator RunIterativeTrainingRoutine()
        {
            // Generate the target list
            PopulateObjectList();

            int numOptions = _selectableSPOs.Count;
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions - 1);
            LogArrayValues(trainArray);

            yield return null;

            // Loop for each training target
            for (int i = 0; i < numTrainingSelections; i++)
            {
                if (selectionCounter >= numSelectionsBeforeTraining)
                {
                    if (updateCounter == 0 || selectionCounter >=
                        numSelectionsBeforeTraining + updateCounter * numSelectionsBetweenTraining)
                    {
                        // update the classifier
                        Debug.Log($"Updating the classifier after {selectionCounter} selections");
                        MarkerWriter.PushUpdateClassifierMarker();
                        updateCounter++;
                    }
                }

                trainTarget = trainArray[i];
                Debug.Log($"Running training selection {i} on object with ID {trainTarget}");

                SPO targetObject = _selectableSPOs[trainTarget];
                yield return RunTrainingRound(
                    DisplayFeedbackWhileWaitingForStimulusToComplete(targetObject),
                    _selectableSPOs[trainTarget], false, true
                );

                selectionCounter++;
            }

            MarkerWriter.PushTrainingCompleteMarker();
        }

        protected override IEnumerator RunSingleTrainingRoutine()
        {
            PopulateObjectList();

            // For the time being, only allow single training on a single object
            if (_selectableSPOs.Count > 1)
            {
                Debug.LogError("Single training only allowed on a single object");
                yield break;
            }

            int trainingIndex = 0;
            SPO targetObject = _selectableSPOs[trainingIndex];

            Debug.Log("Starting single training");
            // For a single, specified SPO, run a single training trial
            if (targetObject != null)
            {
                // Turn on train target - 
                targetObject.OnTrainTarget();
                Debug.Log($"Running single training on option {targetObject.name}");

                // Get the index of the target object
                Debug.Log($"Running single training on option {trainingIndex}");

                // For each window in the trial
                for (int j = 0; j < (numTrainWindows); j++)
                {
                    // Send the marker for the window
                    MarkerWriter.PushMIMarker(1, windowLength, trainingIndex);

                    yield return new WaitForSecondsRealtime(windowLength);

                    if (shamFeedback)
                    {
                        targetObject.Select();
                    }
                }

                // Turn off train target
                targetObject.OffTrainTarget();
            }
            else
            {
                Debug.LogError("No target object specified for single training");
            }

            MarkerWriter.PushTrialEndsMarker();

            yield return null;
        }

        public override void UpdateClassifier()
        {
            Debug.Log("Updating the classifier");
            MarkerWriter.PushTrainingCompleteMarker();
        }

        protected override void SendWindowMarker(int trainingIndex = -1)
        {
            MarkerWriter.PushMIMarker(SPOCount, windowLength, trainingIndex);
        }
    }
}