using BCIEssentials.LSLFramework;

namespace BCIEssentials.Behaviours.Trialing
{
    public abstract class TrialBehaviour : CoroutineBehaviour, IBCIMarkerSource
    {
        public LSLMarkerWriter MarkerWriter { get; set; }
        public bool HasTrainingTarget => TrainingTarget.HasValue;
        public int? TrainingTarget = null;

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
    }
}