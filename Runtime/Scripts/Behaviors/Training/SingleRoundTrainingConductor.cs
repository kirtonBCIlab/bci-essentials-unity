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


        protected override IEnumerator Run() => RunRound(TargetIndex);

        public virtual IEnumerator RunRound(int targetIndex)
        {
            TargetIndicator.BeginTargetIndication(targetIndex);
            yield return new WaitForSeconds(TargetIndicationPeriod);

            if (!PersistTargetIndication)
            {
                TargetIndicator.EndTargetIndication();
            }

            yield return new WaitForSeconds(PreTrialTime);
            TrialConductor.StartTrainingTrial(targetIndex, _lastExecutionHost);
            yield return TrialConductor.AwaitCompletion();
            yield return new WaitForSeconds(PostTrialTime);

            if (PersistTargetIndication)
            {
                TargetIndicator.EndTargetIndication();
            }
        }


        protected override void CleanUp()
        {
            if (TrialConductor.IsRunning) TrialConductor.Interrupt();
        }
    }
}