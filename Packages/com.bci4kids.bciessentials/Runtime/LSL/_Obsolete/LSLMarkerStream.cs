using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public class LSLMarkerStream : MonoBehaviour, IMarkerStream
    {
        public string StreamName = "UnityMarkerStream";
        public string StreamType = "LSL_Marker_Strings";
        public string StreamId = "MyStreamID-Unity1234";

        public StreamOutlet StreamOutlet { get; private set; }

        public string StreamUID { get; private set; } = null;
        
        private readonly string[] _sample = new string[1];

        void Start()
        {
            if (StreamOutlet == null)
            {
                InitializeStream();
            }
        }

        private void OnDestroy()
        {
            StreamOutlet?.Close();
        }

        public bool InitializeStream()
        {
            if (StreamOutlet != null)
            {
                Debug.LogWarning($"Stream already initialized");
                return false;
            }
            
            var streamInfo = new StreamInfo(StreamName, StreamType, 1, 0.0, channel_format_t.cf_string, StreamId);
            StreamOutlet = new StreamOutlet(streamInfo);
            StreamUID = StreamOutlet.info().uid();
            
            return true;
        }

        public void EndStream()
        {
            StreamOutlet?.Close();
        }

        public void Write(string markerString)
        {
            if (StreamOutlet != null || InitializeStream())
            {
                _sample[0] = markerString;
                StreamOutlet.push_sample(_sample);

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