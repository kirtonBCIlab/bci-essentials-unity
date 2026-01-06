using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BCIEssentials.Stimulus.Collections;
using BCIEssentials.Stimulus.Presentation;
using UnityEngine;

namespace BCIEssentials.Behaviours.Trialing.P300
{
    public abstract class MultiFlashTrialBehaviour : P300TrialBehaviour
    {
        protected IEnumerator RunMultiFlash
        (int[] stimulusIndices, List<StimulusPresentationBehaviour> stimulusPresenters)
        {
            int presenterCount = stimulusPresenters.Count;
            List<StimulusPresentationBehaviour> activatedPresenters
            = stimulusIndices.Select(i => stimulusPresenters[i]).ToList();

            activatedPresenters.StartStimulusDisplay();
            SendMultiFlashMarker(stimulusIndices, presenterCount);
            yield return new WaitForSeconds(OnTime);

            activatedPresenters.EndStimulusDisplay();
            yield return new WaitForSeconds(OffTime);
        }
        
        protected void SendMultiFlashMarker
        (IEnumerable<int> stimulusIndices, int optionCount)
        {
            if (MarkerWriter)
            {
                if (HasTrainingTarget)
                {
                    MarkerWriter.PushMultiFlashP300TrainingMarker
                    (optionCount, TrainingTarget.Value, stimulusIndices);
                }
                else
                {
                    MarkerWriter.PushMultiFlashP300ClassificationMarker
                    (optionCount, stimulusIndices);
                }
            }
        }
    }
}