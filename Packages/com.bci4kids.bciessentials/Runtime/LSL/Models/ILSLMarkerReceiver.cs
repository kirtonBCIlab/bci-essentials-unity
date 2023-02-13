using LSL;

namespace BCIEssentials.LSL
{
    public interface ILSLMarkerReceiver
    {
        public string Id { get; }
        public bool Initialized { get; }
        public bool Connected { get; }
        public StreamInfo StreamInfo { get; }
        
        public bool Polling { get; }
        public float PollingFrequency { get; }
        
        public double LastCaptureTime { get; }
        public int ResponsesCount { get; }
        public int SubscriberCount { get; }

        public LSLMarkerReceiver Initialize(string id, StreamInlet streamInlet, LSLMarkerReceiverSettings settings);
        public void CleanUp();
        public void Subscribe(ILSLMarkerSubscriber subscriber);
        public void Unsubscribe(ILSLMarkerSubscriber subscriber);

        public LSLMarkerResponse[] GetResponses();

        public LSLMarkerResponse[] GetLatestResponses();
    }
}