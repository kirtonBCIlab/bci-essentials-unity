using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials
{
    using Extensions;
    using LSLFramework;
    using Stimulus.Collections;
    using Stimulus.Presentation;
    using Utilities;

    public static partial class P300TrialRoutines
    {
        public static IEnumerator RunSingleFlashTrialRoutine
        (
            MarkerWriter markerWriter, int flashesPerOption,
            float onTime, float offTime,
            List<IStimulusPresenter> stimulusPresenters,
            int? trainingTarget = null
        )
        {
            int presenterCount = stimulusPresenters.Count;
            int totalFlashCount = flashesPerOption * presenterCount;

            int[] stimulusOrder = RNRAUtilities.GenerateRNRA_FisherYates
            (totalFlashCount, 0, stimulusPresenters.Count - 1);

            foreach (int stimulusIndex in stimulusOrder)
            {
                yield return RunSingleFlash(
                    stimulusPresenters, stimulusIndex,
                    markerWriter, onTime, offTime, trainingTarget
                );
            }
        }


        private static IEnumerator RunSingleFlash
        (
            List<IStimulusPresenter> stimulusPresenters,
            int stimulusIndex, MarkerWriter markerWriter,
            float onTime, float offTime, int? trainingTarget = null
        )
        {
            int presenterCount = stimulusPresenters.Count;
            IStimulusPresenter target = stimulusPresenters[stimulusIndex];

            target.StartStimulusDisplay();
            markerWriter?.SendSingleFlashMarker
            (stimulusIndex, presenterCount, trainingTarget);
            yield return new WaitForSeconds(onTime);

            target.EndStimulusDisplay();
            yield return new WaitForSeconds(offTime);
        }

        private static IEnumerator RunMultiFlash
        (
            List<IStimulusPresenter> stimulusPresenters,
            int[] stimulusIndices, MarkerWriter markerWriter,
            float onTime, float offTime, int? trainingTarget = null
        )
        {
            int presenterCount = stimulusPresenters.Count;
            List<IStimulusPresenter> activatedPresenters
            = stimulusIndices.Select(i => stimulusPresenters[i]).ToList();

            activatedPresenters.StartStimulusDisplay();
            markerWriter?.SendMultiFlashMarker
            (stimulusIndices, presenterCount, trainingTarget);
            yield return new WaitForSeconds(onTime);

            activatedPresenters.EndStimulusDisplay();
            yield return new WaitForSeconds(offTime);
        }
    }
}