namespace BCIEssentials.LSLFramework
{
    public interface ILSLService
    {
        public bool RegisterMarkerReceiver(LSLMarkerReceiver receiver);

        public LSLMarkerReceiver GetMarkerReceiverByUID(string uid);

        public LSLMarkerReceiver GetMarkerReceiverBySourceId(string sessionId);

        public LSLMarkerReceiver GetMarkerReceiverByPredicate(string predicate);
    }
}