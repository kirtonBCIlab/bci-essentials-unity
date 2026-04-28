using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    using Utilities;

    [System.Serializable]
    public class AutomatedTrainingConductor : SingleRoundTrainingConductor
    {
        [Space]
        public int SelectionCount = 8;
        public float RestTime = 1.0f;

        protected override IEnumerator Run()
        {
            int[] trainArray = RNRAUtilities.GenerateRNRA_FisherYates
            (SelectionCount, 0, TargetIndicator.TargetCount - 1);

            foreach (int targetIndex in trainArray)
            {
                yield return RunRound(targetIndex);
                yield return new WaitForSeconds(RestTime);
            }

            MarkerWriter.PushTrainingCompleteMarker();
        }
    }
}