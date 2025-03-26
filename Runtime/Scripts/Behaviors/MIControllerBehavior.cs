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
        [AppendToFoldoutGroup("Training Properties")]
        [Tooltip("How many selections to make before creating the classifier")]
        public int numSelectionsBeforeTraining = 3;
        [AppendToFoldoutGroup("Training Properties")]
        [Tooltip("How many selections to make before updating the classifier")]
        public int numSelectionsBetweenTraining = 3;

        protected int selectionCounter = 0;
        protected int updateCounter = 0;

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);

            // Warn about the number of objects to be selected from, if greater than 2
            if (_selectableSPOs.Count > 2)
            {
                Debug.LogWarning("Warning: Selecting between more than 2 objects!");
            }
        }

        protected override IEnumerator WhileDoIterativeTraining()
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
                        OutStream.PushUpdateClassifierMarker();
                        updateCounter++;
                    }
                }

                trainTarget = trainArray[i];
                int targetID = _selectableSPOs[trainTarget].ObjectID; 
                Debug.Log($"Running training selection {i} on object with ID {targetID}");

                SPO targetObject = _selectableSPOs[trainTarget];
                yield return RunTrainingRound(
                    DisplayFeedbackWhileWaitingForStimulusToComplete(targetObject),
                    _selectableSPOs[trainTarget], false, true
                );

                selectionCounter++;
            }

            OutStream.PushTrainingCompleteMarker();
        }

        protected override IEnumerator WhileDoSingleTraining()
        {
            PopulateObjectList();

            // For the time being, only allow single training on a single object
            if (_selectableSPOs.Count > 1)
            {
                Debug.LogError("Single training only allowed on a single object");
                yield break;
            }

            SPO targetObject = _selectableSPOs[0];

            Debug.Log("Starting single training");
            // For a single, specified SPO, run a single training trial
            if (targetObject != null)
            {
                // Turn on train target - 
                targetObject.OnTrainTarget();
                Debug.Log($"Running single training on option {targetObject.name}");

                // Get the index of the target object
                int targetID = targetObject.ObjectID;
                Debug.Log($"Running single training on option {targetID}");

                // For each window in the trial
                for (int j = 0; j < (numTrainWindows); j++)
                {
                    // Send the marker for the window
                    OutStream.PushMIMarker(1, windowLength, targetID);

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

            OutStream.PushTrialEndsMarker();

            yield return null;
        }

        /// <summary>
        /// Select an object from <see cref="SelectableSPOs"/> based on the SPO ObjectID,
        /// not the index in the list.
        /// </summary>
        /// <param name="objectIndex"></param>
        /// <param name="stopStimulusRun"></param>
        public override void SelectSPO(int objectID, bool stopStimulusRun = false)
        {
            //If the current training type is not single, then run the base method
            if (CurrentTrainingType != BCITrainingType.Single)
            {
                base.SelectSPO(objectID, stopStimulusRun);
                return;
            }
            
            // If the current training type is single, then make sure the SPO selection is
            // based on the object ID
            var objectCount = _selectableSPOs.Count;
            if (objectCount == 0)
            {
                Debug.LogWarning("No Objects to select");
                return;
            }

            if(_selectableSPOs.TrueForAll(spo => spo.ObjectID != objectID))
            {
                Debug.LogWarning($"ObjectID {objectID} not found in the list of SPOs");
                return;       
            }

            // Select the SPO(s) with the matching ObjectID. Doesn't currently break so has to check all
            foreach (var spo in _selectableSPOs)
            {
                if (spo.ObjectID == objectID)
                {
                    spo.Select();
                    LastSelectedSPO = spo;
                    Debug.Log($"SPO '{spo.gameObject.name}' selected.");
                }
            }
            //I'm unsure if this is needed, but I'm mimicking how the base method handles this
            if (stopStimulusRun)
            {
                StopStimulusRun();
            }

        }

        public override void SelectSPOAtEndOfRun(int objectIndex)
        {
            base.SelectSPOAtEndOfRun(objectIndex);
        }

        public override void UpdateClassifier()
        {
            Debug.Log("Updating the classifier");
            OutStream.PushTrainingCompleteMarker();
        }

        protected override void SendWindowMarker(int trainingIndex = -1)
        {
            if (trainingIndex >= 0 && trainingIndex < SPOCount)
            {
                trainingIndex = _selectableSPOs[trainingIndex].ObjectID;
            }
            OutStream.PushMIMarker(SPOCount, windowLength, trainingIndex);
        }
    }
}