namespace BCIEssentials
{
    using LSLFramework;


    public interface IMarkerSource
    {
        public MarkerWriter MarkerWriter { set; }
    }

    public interface IPredictionSink
    {
        public void OnPrediction(Prediction prediction);
    }
}