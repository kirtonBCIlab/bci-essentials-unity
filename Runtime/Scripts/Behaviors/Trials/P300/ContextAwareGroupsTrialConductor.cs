using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials
{
    using Extensions;
    using Stimulus.Presentation;
    using static Utilities.ContextAwareUtilities;

    [System.Serializable]
    public class ContextAwareGroupsTrialConductor : MultiFlashTrialConductor
    {
        protected override IEnumerator Run()
        {
            for (int i = 0; i < FlashesPerOption; i++)
            {
                List<StimulusPresentationBehaviour> visiblePresenters = PresenterCollection.GetVisibleAndSelectable();

                IEnumerator RunMultiFlashOnVisiblePresenters(int[] stimulusIndices)
                => RunMultiFlash(stimulusIndices, visiblePresenters);

                List<GameObject> presenterObjects = visiblePresenters.SelectGameObjects();
                (int[] subset1, int[] subset2) = CalculateGraphPartition(presenterObjects);
                
                int[,] randomMatrix1 = SubsetToRandomMatrix(subset1);
                int[,] randomMatrix2 = SubsetToRandomMatrix(subset2);

                yield return randomMatrix1.RunForEachRow(RunMultiFlashOnVisiblePresenters);
                yield return randomMatrix2.RunForEachRow(RunMultiFlashOnVisiblePresenters);
                yield return randomMatrix1.RunForEachColumn(RunMultiFlashOnVisiblePresenters);
                yield return randomMatrix2.RunForEachColumn(RunMultiFlashOnVisiblePresenters);
            }
        }
    }
}