using LSL;
using UnityEngine;

namespace BCIEssentials.LSL
{
    public class LSLMarkerStream : MonoBehaviour, IMarkerStream
    {
        public string StreamName = "UnityMarkerStream";
        public string StreamType = "LSL_Marker_Strings";
        public string StreamId = "MyStreamID-Unity1234";

        private StreamOutlet _outlet;
        public StreamOutlet StreamOutlet => _outlet;
        
        private readonly string[] _sample = new string[1];

        void Start()
        {
            if (_outlet == null)
            {
                InitializeStream();
            }
        }

        private void OnDestroy()
        {
            _outlet?.Close();
        }

        public bool InitializeStream()
        {
            if (_outlet != null)
            {
                Debug.LogWarning($"Stream already initialized");
                return false;
            }
            
            var streamInfo = new StreamInfo(StreamName, StreamType, 1, 0.0, channel_format_t.cf_string, StreamId);
            _outlet = new StreamOutlet(streamInfo);

            return _outlet != null;
        }

        public void EndStream()
        {
            _outlet?.Close();
        }

        public void Write(string markerString)
        {
            if (_outlet != null || InitializeStream())
            {
                _sample[0] = markerString;
                _outlet.push_sample(_sample);

                Debug.Log($"Sent Marker : {markerString}");
            }
            else
            {
                Debug.LogError("No stream to write to.");
            }
        }
    }

    public interface IMarkerStream
    {
        public bool InitializeStream();

        public void Write(string markerString);
    }
}