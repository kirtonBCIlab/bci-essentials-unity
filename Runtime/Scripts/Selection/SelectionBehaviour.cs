namespace BCIEssentials.Selection
{
    using LSLFramework;

    public abstract class SelectionBehaviour : MonoBehaviourUsingExtendedAttributes, IPredictionSink
    {
        public abstract void OnPrediction(Prediction prediction);
    }

    public interface IPredictionSink
    {
        public void OnPrediction(Prediction prediction);
    }
}