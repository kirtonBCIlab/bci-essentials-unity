using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials
{
    using Stimulus.Presentation;
    using Utilities;

    [System.Serializable]
    public class RandomFlashTrialConductor : P300TrialConductor
    {
        public RandomFlashTrialConductor(MonoBehaviour executionHost) : base(executionHost) { }

        protected override IEnumerator Run()
        {
            List<StimulusPresentationBehaviour> stimulusPresenters = PresenterCollection.GetSelectable();
            int totalFlashCount = FlashesPerOption * stimulusPresenters.Count;

            int[] stimulusOrder = RNRAUtilities.GenerateRNRA_FisherYates
            (totalFlashCount, 0, stimulusPresenters.Count - 1);

            foreach (int stimulusIndex in stimulusOrder)
            {
                yield return RunSingleFlash(stimulusIndex, stimulusPresenters);
            }
        }

        protected IEnumerator RunSingleFlash
        (int stimulusIndex, List<StimulusPresentationBehaviour> stimulusPresenters)
        {
            StimulusPresentationBehaviour target = stimulusPresenters[stimulusIndex];
            target.StartStimulusDisplay();
            SendSingleFlashMarker(stimulusIndex, stimulusPresenters.Count);
            yield return new WaitForSeconds(OnTime);

            target.EndStimulusDisplay();
            yield return new WaitForSeconds(OffTime);
        }

        protected void SendSingleFlashMarker
        (int stimulusIndex, int presenterCount)
        {
            if (MarkerWriter)
            {
                if (HasTrainingTarget)
                {
                    MarkerWriter.PushSingleFlashP300TrainingMarker
                    (presenterCount, TrainingTarget.Value, stimulusIndex);
                }
                else
                {
                    MarkerWriter.PushSingleFlashP300ClassificationMarker
                    (presenterCount, stimulusIndex);
                }
            }
        }
    }
}