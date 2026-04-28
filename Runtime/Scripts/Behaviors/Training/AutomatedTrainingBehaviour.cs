using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    using Utilities;

    public class AutomatedTrainingConductor : SingleRoundTrainingConductor
    {
        [Space]
        public int SelectionCount = 8;
        public float RestTime = 1.0f;

        public AutomatedTrainingConductor(ITargetIndicator targetIndicator)
        : base(targetIndicator) { }

        protected override IEnumerator Run()
        {
            int[] trainArray = RNRAUtilities.GenerateRNRA_FisherYates(
                SelectionCount, 0, _targetIndicator.TargetCount - 1
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