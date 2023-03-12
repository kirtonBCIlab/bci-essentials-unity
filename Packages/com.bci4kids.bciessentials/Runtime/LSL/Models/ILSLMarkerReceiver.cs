using LSL;

namespace BCIEssentials.LSLFramework
{
    public interface ILSLMarkerReceiver
    {
        public bool Initialized { get; }
        public bool Connected { get; }
        public StreamInfo StreamInfo { get; }
        
        public bool Polling { get; }
        public float PollingFrequency { get; }
        
        public double LastCaptureTime { get; }
        public int ResponsesCount { get; }
        public int SubscriberCount { get; }

        public LSLMarkerReceiver Initialize(string id, StreamInfo streamInfo, LSLMarkerReceiverSettings settings);
        public void CleanUp();
        public void Subscribe(ILSLMarkerSubscriber subscriber);
        public void Unsubscribe(ILSLMarkerSubscriber subscriber);

        public LSLMarkerResponse[] GetResponses();

        public LSLMarkerResponse[] GetLatestResponses();
    }
}