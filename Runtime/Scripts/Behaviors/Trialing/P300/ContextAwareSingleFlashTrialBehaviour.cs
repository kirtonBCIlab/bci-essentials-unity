using System.Collections;
using System.Collections.Generic;
using BCIEssentials.Extensions;
using BCIEssentials.Stimulus.Presentation;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Behaviours.Trialing.P300
{
    using static ContextAwareUtilities;
    public class ContextAwareSingleFlashTrialBehaviour : SingleFlashTrialBehaviour
    {
        private int _lastTourEndNode;

        protected override IEnumerator Run()
        {
            List<IStimulusPresenter> visiblePresenters = PresenterCollection.GetVisible();
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