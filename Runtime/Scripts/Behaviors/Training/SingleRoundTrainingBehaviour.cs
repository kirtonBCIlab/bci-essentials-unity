using System.Collections;
using BCIEssentials.Behaviours.Trialing;
using BCIEssentials.Selection;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public class SingleRoundTrainingBehaviour : TrainingBehaviour, IBCIMarkerSource
    {

        [SerializeField]
        private ISelector _selector;
        [SerializeField]
        private TrialBehaviour _trialBehaviour;

        [StartFoldoutGroup("Training Properties")]
        public int TargetIndex;

        public float TargetIndicationPeriod = 3.0f;
        public bool PersistTargetIndication = false;

        public float PreTrialTime = 0.5f;
        public float PostTrialTime = 0.0f;

        public bool SelectTrainingTarget = false;
        public float TargetSelectionDisplayPeriod = 0.5f;
        [EndFoldoutGroup]
        public float RestTime = 1.0f;


        protected override IEnumerator Run()
        => RunRound(TargetIndex);

        public virtual IEnumerator RunRound(int targetIndex)
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