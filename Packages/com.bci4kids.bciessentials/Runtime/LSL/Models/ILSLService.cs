namespace BCIEssentials.LSLFramework
{
    public interface ILSLService
    {
        public bool RegisterMarkerReceiver(LSLMarkerReceiver receiver);
        public LSLMarkerReceiver GetMarkerReceiver(string markerReceiverId);
        public bool TryGetMarkerReceiverByStreamName(string lslStreamName, out LSLMarkerReceiver markerReceiver);
        public bool TryGetMarkerReceiverByStreamId(string lslStreamId, out LSLMarkerReceiver markerReceiver);
    }
}