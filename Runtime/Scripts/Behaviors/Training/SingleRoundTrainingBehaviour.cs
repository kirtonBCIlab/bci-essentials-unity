using System.Collections;
using BCIEssentials.Behaviours.Trials;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public class SingleRoundTrainingBehaviour : TrainingBehaviour
    {
        [SerializeField]
        protected TargetIndicationBehaviour _targetIndicationBehaviour;
        [SerializeField]
        private TrialBehaviour _trialBehaviour;

        [StartFoldoutGroup("Training Properties")]
        public int TargetIndex;

        public float TargetIndicationPeriod = 3.0f;
        public bool PersistTargetIndication = false;

        public float PreTrialTime = 0.5f;
        [EndFoldoutGroup]
        public float PostTrialTime = 0.0f;


        protected override IEnumerator Run() => RunRound(TargetIndex);

        public virtual IEnumerator RunRound(int targetIndex)
        {
            _targetIndicationBehaviour.BeginTargetIndication(targetIndex);
            yield return new WaitForSeconds(TargetIndicationPeriod);

            if (!PersistTargetIndication)
            {
                _targetIndicationBehaviour.EndTargetIndication();
            }

            yield return new WaitForSeconds(PreTrialTime);
            _trialBehaviour.StartTrainingTrial(targetIndex);
            yield return _trialBehaviour.AwaitCompletion();
            yield return new WaitForSeconds(PostTrialTime);

            if (PersistTargetIndication)
            {
                _targetIndicationBehaviour.EndTargetIndication();
            }
        }


        protected override void CleanUp()
        {
            if (_trialBehaviour.IsRunning) _trialBehaviour.Interrupt();
        }
    }
}