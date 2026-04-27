using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;

    public class SingleRoundTrainingConductor : CoroutineWrapper, IMarkerSource
    {
        public MarkerWriter MarkerWriter { get; set; }
        public TrialConductor TrialConductor;

        [StartFoldoutGroup("Training Properties")]
        public int TargetIndex;

        public float TargetIndicationPeriod = 3.0f;
        public bool PersistTargetIndication = false;

        public float PreTrialTime = 0.5f;
        [EndFoldoutGroup]
        public float PostTrialTime = 0.0f;

        protected ITargetIndicator _targetIndicator;

        public SingleRoundTrainingConductor(
            MonoBehaviour executionHost, ITargetIndicator targetIndicator
        ) : base(executionHost) => _targetIndicator = targetIndicator;


        protected override IEnumerator Run() => RunRound(TargetIndex);

        public virtual IEnumerator RunRound(int targetIndex)
        {
            _targetIndicator.BeginTargetIndication(targetIndex);
            yield return new WaitForSeconds(TargetIndicationPeriod);

            if (!PersistTargetIndication)
            {
                _targetIndicator.EndTargetIndication();
            }

            yield return new WaitForSeconds(PreTrialTime);
            TrialConductor.StartTrainingTrial(targetIndex);
            yield return TrialConductor.AwaitCompletion();
            yield return new WaitForSeconds(PostTrialTime);

            if (PersistTargetIndication)
            {
                _targetIndicator.EndTargetIndication();
            }
        }


        protected override void CleanUp()
        {
            if (TrialConductor.IsRunning) TrialConductor.Interrupt();
        }
    }
}