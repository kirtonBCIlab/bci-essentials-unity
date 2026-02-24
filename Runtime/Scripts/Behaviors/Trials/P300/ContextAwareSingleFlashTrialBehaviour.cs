using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials.Behaviours.Trials.P300
{
    using Extensions;
    using Stimulus.Presentation;
    using static Utilities.ContextAwareUtilities;

    public class ContextAwareSingleFlashTrialBehaviour : SingleFlashTrialBehaviour
    {
        private int _lastTourEndNode;

        protected override IEnumerator Run()
        {
            List<StimulusPresentationBehaviour> visiblePresenters = PresenterCollection.GetVisibleAndSelectable();
            List<GameObject> presenterGameObjects = visiblePresenters.SelectGameObjects();

            for (int i = 0; i < FlashesPerOption; i++)
            {
                int[] stimulusOrder = CalculateGraphTSP
                (presenterGameObjects, ref _lastTourEndNode);

                foreach (int stimulusIndex in stimulusOrder)
                {
                    yield return RunSingleFlash(stimulusIndex, visiblePresenters);
                }
            }
        }
    }
}