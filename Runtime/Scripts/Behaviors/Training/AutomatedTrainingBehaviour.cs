using System.Collections;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public class AutomatedTrainingBehaviour : SingleRoundTrainingBehaviour, IBCIMarkerSource
    {
        [Space]
        public int SelectionCount = 8;

        protected override IEnumerator Run()
        {
            int[] trainArray = RNRAUtilities.GenerateRNRA_FisherYates(
                SelectionCount, 0, _targetIndicationBehaviour.TargetCount - 1
            );

            foreach (int targetIndex in trainArray)
            {
                yield return RunRound(targetIndex);
                yield return new WaitForSeconds(RestTime);
            }

            MarkerWriter.PushTrainingCompleteMarker();
        }
    }
}