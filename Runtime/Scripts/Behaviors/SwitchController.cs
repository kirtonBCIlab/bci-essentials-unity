using System.Collections;
using BCIEssentials.Controllers;
using BCIEssentials.Utilities;
using BCIEssentials.StimulusObjects;
using UnityEngine;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Adds Switch functionality to <see cref="BCIControllerBehavior"/>
    /// </summary>
    public class SwitchControllerBehavior : WindowedControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.Unset;
        
        [Header("Iterative Training")]
        [AppendToFoldoutGroup("Training Properties")]
        [SerializeField]
        [Tooltip("How many selections to make before creating the classifier")]
        private int numSelectionsBeforeTraining = 3;

        [AppendToFoldoutGroup("Training Properties")]
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
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
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

        protected override void SendWindowMarker(int trainingIndex = -1)
        => OutStream.PushSwitchMarker(SPOCount, windowLength, trainingIndex);
    }
}