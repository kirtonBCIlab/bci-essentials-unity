using BCIEssentials.LSLFramework;

namespace BCIEssentials.Behaviours.Training
{
    public abstract class TrainingBehaviour : CoroutineBehaviour, IMarkerSource
    {
        public MarkerWriter MarkerWriter { get; set; }
    }


    public abstract class TargetIndicationBehaviour : MonoBehaviourUsingExtendedAttributes
    {
        public abstract int TargetCount { get; }

        public abstract void BeginTargetIndication(int index);
        public abstract void EndTargetIndication();
    }


    public interface ITargetable
    {
        public void StartTargetIndication();
        public void EndTargetIndication();
    }
}