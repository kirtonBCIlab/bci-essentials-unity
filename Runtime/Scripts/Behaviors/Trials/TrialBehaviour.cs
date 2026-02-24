using BCIEssentials.LSLFramework;

namespace BCIEssentials.Behaviours.Trials
{
    public abstract class TrialBehaviour : CoroutineBehaviour, IMarkerSource
    {
        public MarkerWriter MarkerWriter { get; set; }
        public bool HasTrainingTarget => TrainingTarget.HasValue;
        protected int? TrainingTarget = null;

        public void StartTestingTrial()
        {
            TrainingTarget = null;
            Begin();
        }

        public void StartTrainingTrial(int target)
        {
            TrainingTarget = target;
            Begin();
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