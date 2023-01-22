using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BCIEssentials.ControllerBehaviors
{
     /*
 *

*/

    /// <summary>
    /// Adds Switch functionality to <see cref="BCIControllerBehavior"/>
    /// </summary>
    public class SwitchControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.Unset;
        
        // Variables related to Iterative training
        [SerializeField] [Tooltip("How many selections to make before creating the classifier")]
        private int numSelectionsBeforeTraining = 3;

        [SerializeField] [Tooltip("How many selections to make before updating the classifier")]
        private int numSelectionsBetweenTraining = 3;

        private int selectionCounter = 0;
        private int updateCounter = 0;

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);

            // Warn about the number of objects to be selected from, if greater than 2
            if (objectList.Count > 2)
            {
                print("Warning: Selecting between more than 2 objects!");
            }
        }

        // Coroutine for the stimulus, wait there is no stimulus
        public override IEnumerator Stimulus()
        {
            yield return 0;
        }

        public override IEnumerator DoIterativeTraining()
        {
            // Generate the target list
            PopulateObjectList();

            int numOptions = objectList.Count;

            // Create a random non repeating array 
            int[] trainArray = ArrayUtilities.GenerateRNRA(numTrainingSelections, 0, numOptions);
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
                        marker.Write("Update Classifier");
                        updateCounter++;
                    }
                }

                // Get the target from the array
                trainTarget = trainArray[i];

                // 
                Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

                // Turn on train target
                objectList[trainTarget].OnTrainTarget();

                // Go through the training sequence
                yield return new WaitForSecondsRealtime(pauseBeforeTraining);


                StimulusOn();
                for (int j = 0; j < (numTrainWindows - 1); j++)
                {
                    yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);

                    if (shamFeedback)
                    {
                        objectList[trainTarget].Select();
                    }
                }

                StimulusOff();

                // Take a break
                yield return new WaitForSecondsRealtime(trainBreak);

                // Turn off train target
                objectList[trainTarget].OffTrainTarget();

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

        public override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (stimOn)
            {
                // Desired format is: [mi, number of options, training label (or -1 if n/a), window length] 
                string trainingString = trainingIndex <= objectList.Count ? trainingIndex.ToString() : "-1";

                // Send the marker
                marker.Write($"switch, {objectList.Count}, {trainingString}, {windowLength}");

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }
    }
}