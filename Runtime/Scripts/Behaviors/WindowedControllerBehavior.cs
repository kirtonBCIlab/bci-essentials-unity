using System.Collections;
using UnityEngine;
using BCIEssentials.StimulusObjects;
using BCIEssentials.Utilities;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Base controller behavior class for paradigms that send
    /// constant window markers over the course of a trial.
    /// </summary>
    public abstract class WindowedControllerBehavior: BCIControllerBehavior
    {

        [ShowWithFoldoutGroup("Training Properties")]
        [Tooltip("The number of windows used in each training iteration")]
        public int numTrainWindows = 3;

        [FoldoutGroup("Signal Properties")]
        [Tooltip("The length of the processing window [sec]")]
        public float windowLength = 1.0f;
        [EndFoldoutGroup]
        [Tooltip("The interval between processing windows [sec]")]
        public float interWindowInterval = 0f;


        public override void StartStimulusRun()
        {
            base.StartStimulusRun();
            StopStartCoroutine(ref _sendMarkers,
                RunSendWindowMarkers(trainTarget)
            );
        }
        
        private IEnumerator RunSendWindowMarkers(int trainingIndex = 99)
        {
            while (StimulusRunning)
            {
                // Send the marker
                if (OutStream != null) SendWindowMarker(trainingIndex);
                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }

        protected virtual void SendWindowMarker(int trainingIndex = -1) {}

                
        // Do training
        protected override IEnumerator WhileDoAutomatedTraining()
        {
            // Generate the target list
            PopulateObjectList();

            // Get number of selectable objects by counting the objects in the objectList
            int numOptions = _selectableSPOs.Count;

            // Create a random non repeating array 
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(numTrainingSelections, 0, numOptions-1);
            LogArrayValues(trainArray);

            yield return new WaitForSecondsRealtime(0.001f);

            // Loop for each training target
            for (int i = 0; i < numTrainingSelections; i++)
            {
                // Get the target from the array
                trainTarget = trainArray[i];

                // 
                Debug.Log("Running training selection " + i.ToString() + " on option " + trainTarget.ToString());

                // Turn on train target
                _selectableSPOs[trainTarget].GetComponent<SPO>().OnTrainTarget();


                yield return new WaitForSecondsRealtime(trainTargetPresentationTime);

                if (trainTargetPersistent == false)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }

                yield return new WaitForSecondsRealtime(0.5f);

                // Go through the training sequence
                //yield return new WaitForSecondsRealtime(3f);

                StartStimulusRun();
                yield return new WaitForSecondsRealtime((windowLength + interWindowInterval) * (float)numTrainWindows);
                StopStimulusRun();

                // Turn off train target
                if (trainTargetPersistent == true)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().OffTrainTarget();
                }


                // If sham feedback is true, then show it
                if (shamFeedback)
                {
                    _selectableSPOs[trainTarget].GetComponent<SPO>().Select();
                }

                trainTarget = -1;

                // Take a break
                yield return new WaitForSecondsRealtime(trainBreak);
            }

            OutStream.PushTrainingCompleteMarker();
        }
    }
}