using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials
{
    using Extensions;
    using Stimulus.Collections;
    using Stimulus.Presentation;

    public abstract class MultiFlashTrialConductor : P300TrialConductor
    {
        protected IEnumerator RunMultiFlash
        (int[] stimulusIndices, List<StimulusPresentationBehaviour> stimulusPresenters)
        {
            int presenterCount = stimulusPresenters.Count;
            stimulusIndices = stimulusIndices.WherePositiveAndLessThan(presenterCount);

            List<StimulusPresentationBehaviour> activatedPresenters
            = stimulusIndices.Select(i => stimulusPresenters[i]).ToList();

            activatedPresenters.StartStimulusDisplay();
            SendMultiFlashMarker(stimulusIndices, presenterCount);
            yield return new WaitForSeconds(OnTime);

            activatedPresenters.EndStimulusDisplay();
            yield return new WaitForSeconds(OffTime);
        }

        protected void SendMultiFlashMarker
        (IEnumerable<int> stimulusIndices, int presenterCount)
        {
            if (MarkerWriter)
            {
                if (HasTrainingTarget)
                {
                    MarkerWriter.PushMultiFlashP300TrainingMarker
                    (presenterCount, TrainingTarget.Value, stimulusIndices);
                }
                else
                {
                    MarkerWriter.PushMultiFlashP300ClassificationMarker
                    (presenterCount, stimulusIndices);
                }
            }
        }
    }
}