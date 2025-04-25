using System.Collections;
using UnityEngine;
using BCIEssentials.StimulusObjects;

namespace BCIEssentials.ControllerBehaviors
{
    /// <summary>
    /// Base controller behavior class for paradigms that send
    /// constant epoch markers over the course of a trial.
    /// </summary>
    public abstract class ContinualStimulusControllerBehavior: BCIControllerBehavior
    {

        [AppendToFoldoutGroup("Training Properties")]
        [Tooltip("The number of epochs used in each training iteration")]
        public int trainingEpochCount = 3;

        [StartFoldoutGroup("Signal Properties")]
        [Tooltip("The length of the processing epoch [sec]")]
        public float epochLength = 1.0f;
        [EndFoldoutGroup]
        [Tooltip("The interval between processing epochs [sec]")]
        public float interEpochInterval = 0f;
        
        private Coroutine _epochMarkerCoroutine;


        protected override void SetUpForStimulusRun()
        {
            StopStartCoroutine(ref _epochMarkerCoroutine,
                RunSendEpochMarkers(trainTarget)
            );
        }

        protected override void CleanUpAfterStimulusRun()
        {
            StopCoroutineReference(ref _epochMarkerCoroutine);
        }
        
        private IEnumerator RunSendEpochMarkers(int trainingIndex = 99)
        {
            while (true)
            {
                // Send the marker
                if (MarkerWriter != null)
                {
                    if (TrainingRunning)
                        SendTrainingMarker(trainingIndex);
                    else
                        SendClassificationMarker();
                }
                // Wait the epoch length + the inter-epoch interval
                yield return new WaitForSecondsRealtime(epochLength + interEpochInterval);
            }
        }

        protected abstract void SendTrainingMarker(int trainingIndex);
        protected abstract void SendClassificationMarker();


        protected override IEnumerator RunStimulusRoutine()
        {
            while (true)
            {
                UpdateStimulus();
                yield return null;
            }
        }

        protected virtual void UpdateStimulus() {}

        protected override IEnumerator WaitForStimulusToComplete()
        {
            yield return new WaitForSecondsRealtime(
                (epochLength + interEpochInterval) * trainingEpochCount
            );
            StopStimulusRun();
        }

        protected IEnumerator DisplayFeedbackWhileWaitingForStimulusToComplete
        (SPO feedbackTarget)
        {
            for (int i = 0; i < trainingEpochCount; i++)
            {
                yield return new WaitForSecondsRealtime(epochLength + interEpochInterval);

                if (shamFeedback)
                {
                    feedbackTarget.Select();
                }
            }
        }
    }
}