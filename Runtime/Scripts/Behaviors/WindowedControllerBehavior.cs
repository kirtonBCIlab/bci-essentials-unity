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

        [AppendToFoldoutGroup("Training Properties")]
        [Tooltip("The number of windows used in each training iteration")]
        public int numTrainWindows = 3;

        [StartFoldoutGroup("Signal Properties")]
        [Tooltip("The length of the processing window [sec]")]
        public float windowLength = 1.0f;
        [EndFoldoutGroup]
        [Tooltip("The interval between processing windows [sec]")]
        public float interWindowInterval = 0f;
        
        protected Coroutine _sendMarkers;


        protected override void SetupUpForStimulusRun()
        {
            StopStartCoroutine(ref _sendMarkers,
                RunSendWindowMarkers(trainTarget)
            );
        }

        protected override void CleanUpAfterStimulusRun()
        {
            StopCoroutineReference(ref _sendMarkers);
        }
        
        private IEnumerator RunSendWindowMarkers(int trainingIndex = 99)
        {
            while (StimulusRunning)
            {
                // Send the marker
                if (MarkerWriter != null) SendWindowMarker(trainingIndex);
                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }

        protected abstract void SendWindowMarker(int trainingIndex = -1);

        protected override IEnumerator WaitForStimulusToComplete()
        {
            yield return new WaitForSecondsRealtime(
                (windowLength + interWindowInterval) * numTrainWindows
            );
        }

        protected IEnumerator DisplayFeedbackWhileWaitingForStimulusToComplete
        (SPO feedbackTarget)
        {
            for (int i = 0; i < numTrainWindows; i++)
            {
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);

                if (shamFeedback)
                {
                    feedbackTarget.Select();
                }
            }
        }
    }
}