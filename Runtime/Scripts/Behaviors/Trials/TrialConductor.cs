using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;

    public abstract class TrialConductor : CoroutineWrapper, IMarkerSource
    {
        public MarkerWriter MarkerWriter { get; set; }
        public bool HasTrainingTarget => TrainingTarget.HasValue;
        protected int? TrainingTarget = null;


        public void StartTestingTrial(MonoBehaviour executionHost)
        {
            TrainingTarget = null;
            Begin(executionHost);
        }

        public void StartTrainingTrial(int target, MonoBehaviour executionHost)
        {
            TrainingTarget = target;
            Begin(executionHost);
        }

        protected override void SetUp()
        {
            MarkerWriter.PushTrialStartedMarker();
        }
        protected override void CleanUp()
        {
            MarkerWriter.PushTrialEndsMarker();
            ClearTrainingTarget();
        }

        public void ClearTrainingTarget() => TrainingTarget = null;
    }
}