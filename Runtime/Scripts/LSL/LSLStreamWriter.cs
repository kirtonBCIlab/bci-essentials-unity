using LSL;
using UnityEngine;
using static System.Diagnostics.Process;

namespace BCIEssentials.LSLFramework
{
    public class LSLStreamWriter: MonoBehaviour
    {
        public string StreamName = "UnityMarkerStream";
        public string StreamType = "LSL_Marker_Strings";
        public bool PrintLogs = false;

        public bool HasConsumers => _outlet?.have_consumers() ?? false;
        protected bool HasLiveOutlet => _outlet is not null;
        private StreamOutlet _outlet;


        void Start()
        {
            if (!HasLiveOutlet)
                OpenStream();
        }

        void OnDestroy() => CloseStream();


        public bool OpenStream()
        {
            if (HasLiveOutlet)
            {
                Debug.LogWarning($"Stream already initialized");
                return false;
            }
            
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            int processID = GetCurrentProcess().Id;
            var streamInfo = new StreamInfo
            (
                StreamName, StreamType,
                channel_format: channel_format_t.cf_string,
                source_id: $"{deviceID}-Unity-{processID}"
            );
            _outlet = new StreamOutlet(streamInfo);
            
            return true;
        }

        public void CloseStream()
        {
            _outlet?.Close();
            _outlet = null;
        }


        public void PushString(string s)
        {
            if (HasLiveOutlet || OpenStream())
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