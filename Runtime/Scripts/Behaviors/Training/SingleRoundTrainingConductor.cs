using System.Collections;
using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;

    [System.Serializable]
    public class SingleRoundTrainingConductor : CoroutineWrapper, IMarkerSource
    {
        public MarkerWriter MarkerWriter { get; set; }
        public ITargetIndicator TargetIndicator;
        public TrialConductor TrialConductor;

        [StartFoldoutGroup("Training Properties")]
        public int TargetIndex;

        public float TargetIndicationPeriod = 3.0f;
        public bool PersistTargetIndication = false;

        public float PreTrialTime = 0.5f;
        [EndFoldoutGroup]
        public float PostTrialTime = 0.0f;

        protected bool _targetIsBeingIndicated;


        protected override IEnumerator Run() => RunRound(TargetIndex);

        public virtual IEnumerator RunRound(int targetIndex)
        {
            TargetIndicator.BeginTargetIndication(targetIndex);
            _targetIsBeingIndicated = true;
            try
            {
                yield return new WaitForSeconds(TargetIndicationPeriod);

                if (!PersistTargetIndication) EndAndFlagTargetIndication();

                yield return new WaitForSeconds(PreTrialTime);
                TrialConductor.StartTrainingTrial(targetIndex, _lastExecutionHost);
                yield return TrialConductor.AwaitCompletion();
                yield return new WaitForSeconds(PostTrialTime);

                if (PersistTargetIndication) EndAndFlagTargetIndication();
            }
            finally
            {
                if (_targetIsBeingIndicated) EndAndFlagTargetIndication();
            }
        }


        protected override void CleanUp()
        {
            if (TrialConductor.IsRunning) TrialConductor.Interrupt();
            if (_targetIsBeingIndicated) EndAndFlagTargetIndication();
        }

        protected void EndAndFlagTargetIndication()
        {
            TargetIndicator.EndTargetIndication();
            _targetIsBeingIndicated = false;
        }
    }
}