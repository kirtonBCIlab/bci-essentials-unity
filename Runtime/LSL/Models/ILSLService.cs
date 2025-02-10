namespace BCIEssentials.LSLFramework
{
    public interface ILSLService
    {
        public bool RegisterMarkerReceiver(ILSLMarkerReceiver receiver);
        public bool HasRegisteredMarkerReceiver(ILSLMarkerReceiver receiver);
        
        public ILSLMarkerReceiver GetMarkerReceiverByUID(string uid);

        public ILSLMarkerReceiver GetMarkerReceiverByName(string streamName);

        public ILSLMarkerReceiver GetMarkerReceiverByPredicate(string predicate);
        public void UnregisterMarkerReceiver(ILSLMarkerReceiver receiver);
    }
}