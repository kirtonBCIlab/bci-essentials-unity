using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials
{
    using Extensions;
    using LSLFramework;
    using Stimulus;
    using static Utilities.ContextAwareUtilities;

    public static partial class P300TrialRoutines
    {
        public static IEnumerator RunContextAwareFlashRoutine
        (
            MarkerWriter markerWriter, int flashesPerOption,
            float onTime, float offTime,
            List<IStimulusPresenter> stimulusPresenters,
            int? trainingTarget = null
        )
        {
            List<IStimulusPresenter> visiblePresenters = stimulusPresenters.WhereVisibleFromMainCamera();
            List<GameObject> presenterObjects = visiblePresenters.SelectGameObjects();

            for (int i = 0; i < flashesPerOption; i++)
            {
                int[] stimulusOrder = CalculateGraphTSP(presenterObjects);

                foreach (int stimulusIndex in stimulusOrder)
                {
                    yield return RunSingleFlash(
                        visiblePresenters, stimulusIndex,
                        markerWriter, onTime, offTime, trainingTarget
                    );
                }
            }
        }

        public static IEnumerator RunContextAwareGroupsFlashRoutine
        (
            MarkerWriter markerWriter, int flashesPerOption,
            float onTime, float offTime,
            List<IStimulusPresenter> stimulusPresenters,
            int? trainingTarget = null
        )
        {
            List<IStimulusPresenter> visiblePresenters = stimulusPresenters.WhereVisibleFromMainCamera();
            List<GameObject> presenterObjects = visiblePresenters.SelectGameObjects();

            for (int i = 0; i < flashesPerOption; i++)
            {
                IEnumerator RunBoundMultiFlash(int[] stimulusIndices)
                => RunMultiFlash(
                    visiblePresenters, stimulusIndices,
                    markerWriter, onTime, offTime, trainingTarget
                );

                (int[] subset1, int[] subset2) = CalculateGraphPartition(presenterObjects);

                int[,] randomMatrix1 = SubsetToRandomMatrix(subset1);
                int[,] randomMatrix2 = SubsetToRandomMatrix(subset2);

                yield return randomMatrix1.RunForEachRow(RunBoundMultiFlash);
                yield return randomMatrix2.RunForEachRow(RunBoundMultiFlash);
                yield return randomMatrix1.RunForEachColumn(RunBoundMultiFlash);
                yield return randomMatrix2.RunForEachColumn(RunBoundMultiFlash);
            }
        }
    }
}