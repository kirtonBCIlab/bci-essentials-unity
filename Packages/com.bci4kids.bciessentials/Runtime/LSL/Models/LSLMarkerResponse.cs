namespace BCIEssentials.LSL
{
    public class LSLMarkerResponse
    {
        public LSLMarkerResponse(double captureTime, string[] value)
        {
            CaptureTime = captureTime;
            Value = value;
        }

        /// <summary>
        /// Capture time of the response value by the lsl stream.
        /// </summary>
        public double CaptureTime { get; }
        
        /// <summary>
        /// Response value.
        /// </summary>
        public string[] Value { get; }
    }
}