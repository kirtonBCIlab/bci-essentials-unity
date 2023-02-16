using System.Collections;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.ControllerBehaviors
{
    public class MIControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.MI;

        // Variables related to Iterative training
        public int numSelectionsBeforeTraining = 3; // How many selections to make before creating the classifier
        public int numSelectionsBetweenTraining = 3; // How many selections to make before updating the classifier


        protected int selectionCounter = 0;
        protected int updateCounter = 0;

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
            int[] trainArray = new int[numTrainingSelections];
            trainArray = ArrayUtilities.GenerateRNRA(numTrainingSelections, 0, numOptions);
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
                        marker.Write("Update Classifier");
                        updateCounter++;
                    }
                    else if (selectionCounter >=
                             numSelectionsBeforeTraining + (updateCounter * numSelectionsBetweenTraining))
                    {
                        // update the classifier
                        Debug.Log($"Updating the classifier after {selectionCounter} selections");
                        marker.Write("Update Classifier");
                        updateCounter++;
                    }
                }

                // Get the target from the array
                trainTarget = trainArray[i];

                // 
                Debug.Log($"Running training selection {i} on option {trainTarget}");

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
            marker.Write("Training Complete");

            yield return 0;

        }

        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (StimulusRunning)
            {
                // Desired format is: [mi, number of options, training label (or -1 if n/a), window length] 
                string trainingString = trainingIndex <= _selectableSPOs.Count ? trainingIndex.ToString() : "-1";

                // Send the marker
                marker.Write($"mi, {_selectableSPOs.Count}, {trainingString}, {windowLength}");

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);


            }
        }
    }
}