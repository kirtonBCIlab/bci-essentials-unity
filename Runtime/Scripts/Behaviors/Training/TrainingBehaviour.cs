using BCIEssentials.LSLFramework;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public abstract class TrainingBehaviour : CoroutineBehaviour, IBCIMarkerSource
    {
        public LSLMarkerWriter MarkerWriter { get; set; }

        [SerializeField]
        protected ITrainingTargetIndicator TargetIndicator;
    }


    public interface ITrainingTargetIndicator
    {
        public int OptionCount { get; }
        public void BeginTargetIndication(int index);
        public void EndTargetIndication();
    }


    public interface ITargetable
    {
        public void StartTargetIndication();
        public void EndTargetIndication();
    }
}