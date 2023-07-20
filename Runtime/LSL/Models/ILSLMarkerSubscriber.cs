namespace BCIEssentials.LSLFramework
{
    public interface ILSLMarkerSubscriber
    {
        public void NewMarkersCallback(LSLMarkerResponse[] latestMarkers);
    }
}