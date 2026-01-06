using BCIEssentials.LSLFramework;
using BCIEssentials.Selection;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public abstract class TrainingBehaviour : CoroutineBehaviour, IBCIMarkerSource
    {
        public LSLMarkerWriter MarkerWriter { get; set; }

        [SerializeField]
        protected TargetIndicationBehaviour _targetIndicationBehaviour;
    }


    public abstract class TargetIndicationBehaviour : SelectionBehaviour
    {
        public abstract int OptionCount { get; }

        public abstract void BeginTargetIndication(int index);
        public abstract void EndTargetIndication();
    }


    public interface ITargetable
    {
        public void StartTargetIndication();
        public void EndTargetIndication();
    }
}