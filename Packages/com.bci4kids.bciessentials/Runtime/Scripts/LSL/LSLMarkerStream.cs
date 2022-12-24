using LSL;
using UnityEngine;

namespace BCIEssentials
{
    public class LSLMarkerStream : MonoBehaviour
    {
        private StreamOutlet outlet;

        public string StreamName = "UnityMarkerStream";
        public string StreamType = "LSL_Marker_Strings";
        public string StreamId = "MyStreamID-Unity1234";

        private string[] sample = new string[1];

        void Start()
        {
            StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, 1, 0.0, channel_format_t.cf_string);

            outlet = new StreamOutlet(streamInfo);
        }

        public void Write(string markerString)
        {
            sample[0] = markerString;
            outlet.push_sample(sample);

            Debug.Log("Sent Marker : " + markerString);
        }

    }
}