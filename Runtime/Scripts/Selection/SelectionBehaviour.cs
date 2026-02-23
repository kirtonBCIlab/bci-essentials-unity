using BCIEssentials.LSLFramework;

namespace BCIEssentials.Selection
{
    public abstract class SelectionBehaviour : MonoBehaviourUsingExtendedAttributes, IPredictionSink
    {
        public abstract void OnPrediction(Prediction prediction);
    }

    public interface IPredictionSink
    {
        public void OnPrediction(Prediction prediction);
    }
}