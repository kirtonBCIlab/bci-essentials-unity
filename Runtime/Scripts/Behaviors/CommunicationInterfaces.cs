namespace BCIEssentials
{
    using LSLFramework;


    public interface IHasInstanceID
    {
        public int GetInstanceID();
    }


    public interface IMarkerSource : IHasInstanceID
    {
        public MarkerWriter MarkerWriter { set; }
    }

    public interface IPredictionSink : IHasInstanceID
    {
        public void OnPrediction(Prediction prediction);
    }
}