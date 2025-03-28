using System.Collections;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Adds Switch functionality to <see cref="BCIControllerBehavior"/>
    /// </summary>
    public class SwitchControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.Unset;
        
        [Header("Iterative Training")]
        [ShowWithFoldoutGroup("Training Properties")]
        [SerializeField]
        [Tooltip("How many selections to make before creating the classifier")]
        private int numSelectionsBeforeTraining = 3;

        [ShowWithFoldoutGroup("Training Properties")]
        [SerializeField]
        [Tooltip("How many selections to make before updating the classifier")]
        private int numSelectionsBetweenTraining = 3;

        private int selectionCounter = 0;
        private int updateCounter = 0;

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);

            // Warn about the number of objects to be selected from, if greater than 2
            if (_selectableSPOs.Count > 2)
            {
                print("Warning: Selecting between more than 2 objects!");
            }
        }

        protected override IEnumerator WhileDoIterativeTraining()
        {
            // Generate the target list
            PopulateObjectList();

            int numOptions = _selectableSPOs.Count;

            // Create a random non repeating array 
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
            LogArrayValues(trainArray);

            yield return 0;


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

                // Get the target from the array
                trainTarget = trainArray[i];

                // 
                Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

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

        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (StimulusRunning)
            {
                // Send the marker
                OutStream.PushSwitchMarker(SPOCount, windowLength, trainingIndex);

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }
    }
}