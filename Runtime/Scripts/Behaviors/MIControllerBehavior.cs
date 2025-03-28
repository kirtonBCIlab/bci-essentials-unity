using System.Collections;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.ControllerBehaviors
{
    public class MIControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.MI;

        [Header("Iterative Training")]
        [ShowWithFoldoutGroup("Training Properties")]
        [Tooltip("How many selections to make before creating the classifier")]
        public int numSelectionsBeforeTraining = 3;
        [ShowWithFoldoutGroup("Training Properties")]
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

        public override void StartStimulusRun(bool sendConstantMarkers = true)
        {
            if (StimulusRunning)
            {
                StopStimulusRun();
            }
            
            StimulusRunning = true;
            LastSelectedSPO = null;
            
            // Send the marker to start
            OutStream.PushTrialStartedMarker();

            StartReceivingMarkers();
            PopulateObjectList();
            StopStartCoroutine(ref _runStimulus, RunStimulus());

            // Not required for P300
            if (sendConstantMarkers)
            {
                // If trainTarget == -1, then we are trying to classify, pass -1 along
                if (trainTarget == 99)
                {
                    StopStartCoroutine(ref _sendMarkers, SendMarkers(-1));
                }
                // Otherwise, pass the objectID of the target
                else
                {
                    StopStartCoroutine(ref _sendMarkers, SendMarkers(_selectableSPOs[trainTarget].ObjectID));
                }
            }
        }

        protected override IEnumerator WhileDoIterativeTraining()
        {
            // Generate the target list
            PopulateObjectList();

            int numOptions = _selectableSPOs.Count;

            // Create a random non repeating array 
            int[] trainArray = new int[numTrainingSelections];
            trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
            LogArrayValues(trainArray);

            yield return 0;


            // Loop for each training target
            for (int i = 0; i < numTrainingSelections; i++)
            {

                if (selectionCounter >= numSelectionsBeforeTraining)
                {
                    if (updateCounter == 0)
                    {
                        // update the classifier
                        Debug.Log($"Updating the classifier after {selectionCounter} selections");
                        OutStream.PushUpdateClassifierMarker();
                        updateCounter++;
                    }
                    else if (selectionCounter >=
                             numSelectionsBeforeTraining + (updateCounter * numSelectionsBetweenTraining))
                    {
                        // update the classifier
                        Debug.Log($"Updating the classifier after {selectionCounter} selections");
                        OutStream.PushUpdateClassifierMarker();
                        updateCounter++;
                    }
                }

                // Get the target from the array
                trainTarget = trainArray[i];

                // Get the index of the target object
                int targetID = _selectableSPOs[trainTarget].ObjectID; 

                // 
                Debug.Log($"Running training selection {i} on option {targetID}");

                // Turn on train target
                _selectableSPOs[trainTarget].OnTrainTarget();

                // Go through the training sequence
                yield return new WaitForSecondsRealtime(pauseBeforeTraining);


                StartStimulusRun();
                for (int j = 0; j < (numTrainWindows - 1); j++)
                {
                    yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);

                    if (shamFeedback)
                    {
                        _selectableSPOs[trainTarget].Select();
                    }
                }

                StopStimulusRun();

                // Take a break
                yield return new WaitForSecondsRealtime(trainBreak);

                // Turn off train target
                _selectableSPOs[trainTarget].OffTrainTarget();

                // Reset objects

                // Take a break
                yield return new WaitForSecondsRealtime(trainBreak);

                trainTarget = 99;
                selectionCounter++;
            }

            // Send marker
            OutStream.PushTrainingCompleteMarker();

            yield return 0;

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

        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (StimulusRunning)
            {
                // Send the marker
                OutStream.PushMIMarker(SPOCount, windowLength, trainingIndex);

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }
    }
}