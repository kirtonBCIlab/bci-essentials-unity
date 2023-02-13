namespace BCIEssentials.LSL
{
    public interface ILSLMarkerSubscriber
    {
        public void NewMarkersCallback(LSLMarkerResponse[] latestMarkers);
    }
}