using System.Collections;
using BCIEssentials.Behaviours;
using BCIEssentials.LSLFramework;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Training
{
    public class AutomatedBCITrainingBehaviour : BCITrainingBehaviour, IBCIMarkerSource
    {
        public LSLMarkerWriter MarkerWriter { get; set; }

        public int SelectionCount = 8;
        public float TargetIndicationPeriod = 3.0f;
        public bool PersistTargetIndication = false;

        public float PreTrialTime = 0.5f;
        public float PostTrialTime = 0.0f;

        public bool SelectTrainingTarget = false;
        public float TargetSelectionDisplayPeriod = 0.5f;

        public float RestTime = 1.0f;

        [SerializeField]
        private IBCISelector _selector;
        [SerializeField]
        private CoroutineWrapper _trialBehaviour;


        protected override IEnumerator Run()
        {
            int[] trainArray = ArrayUtilities.GenerateRNRA_FisherYates(
                SelectionCount, 0, TargetIndicator.OptionCount
            );

            foreach (int targetIndex in trainArray)
            {
                yield return RunRound(targetIndex);
                yield return new WaitForSeconds(RestTime);
            }

            MarkerWriter.PushTrainingCompleteMarker();
        }


        protected virtual IEnumerator RunRound(int targetIndex)
        {
            TargetIndicator.BeginTargetIndication(targetIndex);
            yield return new WaitForSeconds(TargetIndicationPeriod);

            if (!PersistTargetIndication)
            {
                TargetIndicator.EndTargetIndication();
            }

            yield return new WaitForSeconds(PreTrialTime);
            _trialBehaviour.Begin();
            yield return _trialBehaviour.AwaitCompletion();
            yield return new WaitForSeconds(PostTrialTime);

            if (SelectTrainingTarget && _selector != null)
            {
                _selector.MakeSelection(targetIndex);
                yield return new WaitForSeconds(TargetSelectionDisplayPeriod);
            }

            if (PersistTargetIndication)
            {
                TargetIndicator.EndTargetIndication();
            }
        }
    }
}