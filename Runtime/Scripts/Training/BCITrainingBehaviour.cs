using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Training
{
    public abstract class BCITrainingBehaviour : CoroutineWrapper
    {
        [SerializeField]
        protected IBCITrainingTargetIndicator TargetIndicator;
    }


    public interface IBCITrainingTargetIndicator
    {
        public int OptionCount { get; }
        public void BeginTargetIndication(int index);
        public void EndTargetIndication();
    }
}