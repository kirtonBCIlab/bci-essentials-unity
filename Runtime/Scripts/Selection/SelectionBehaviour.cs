namespace BCIEssentials.Selection
{
    using BCIEssentials.Behaviours;
    using LSLFramework;

    public abstract class SelectionBehaviour : MonoBehaviourUsingExtendedAttributes, IPredictionSink
    {
        public abstract void OnPrediction(Prediction prediction);
    }

    public interface IPredictionSink : IHasInstanceID
    {
        public void OnPrediction(Prediction prediction);
    }
}