using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public class LSLOutletHost: MonoBehaviour
    {
        public string StreamName = "UnityMarkerStream";
        public string StreamType = "LSL_Marker_Strings";
        public bool PrintLogs = false;

        public bool HasConsumers
            => _outlet is not null
            && _outlet.have_consumers();
        private StreamOutlet _outlet;


        void Start()
        {
            if (_outlet is null)
                OpenStream();
        }

        void OnDestroy() => CloseStream();


        public bool OpenStream()
        {
            if (_outlet is not null)
            {
                Debug.LogWarning($"Stream already initialized");
                return false;
            }
            
            var streamInfo = new StreamInfo
            (
                StreamName, StreamType,
                channel_format: channel_format_t.cf_string,
                source_id: $"{SystemInfo.deviceUniqueIdentifier}-Unity"
            );
            _outlet = new StreamOutlet(streamInfo);
            
            return true;
        }

        public void CloseStream() => _outlet?.Close();


        public void PushString(string s)
        {
            if (_outlet is not null || OpenStream())
            {
                _outlet.push_sample(new[] {s});
                if (PrintLogs)
                {
                    Debug.Log($"Sent Marker: {s}");
                }
            }
            else
            {
                Debug.LogError("No outlet to write to");
            }
        }
    }
}