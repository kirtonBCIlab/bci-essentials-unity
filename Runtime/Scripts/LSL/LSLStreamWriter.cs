using System;
using System.Collections.Generic;
using System.Linq;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public class LSLStreamWriter: MonoBehaviour
    {
        public string StreamName = "UnityMarkerStream";
        public string StreamType = "BCI_Essentials_Markers";
        public bool PrintLogs = false;

        public bool HasConsumers => _outlet?.have_consumers() ?? false;
        public bool HasLiveOutlet => _outlet is not null;
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

            ThrowExceptionIfDuplicateWriterIsLive();
            WarnIfTypeIsReused();

            var streamInfo = new StreamInfo
            (
                StreamName, StreamType,
                channel_format: channel_format_t.cf_string,
                source_id: BuildSourceID()
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
                _outlet.push_sample(new[] { s });
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


        private bool WriterSharesNameAndType(LSLStreamWriter other)
        => other.StreamName == StreamName && WriterSharesType(other);
        private bool WriterSharesType(LSLStreamWriter other)
        => other.StreamType == StreamType;

        private void ThrowExceptionIfDuplicateWriterIsLive()
        {
            if (AnyOtherLiveStreamWriters(WriterSharesNameAndType))
            throw new Exception(
                "Another Stream Writer with the same name and type is "
                + "already live, opening this one would duplicate streams"
            );
        }

        private void WarnIfTypeIsReused()
        {
            if (AnyOtherLiveStreamWriters(WriterSharesType))
            Debug.LogWarning(
                "Another Stream Writer with the same type is already live, "
                + "beware that a standard back end will only see "
                + "the first of these and ignore any others"
            );
        }

        private bool AnyOtherLiveStreamWriters(Func<LSLStreamWriter, bool> predicate)
        {
            LSLStreamWriter[] streamWritersInScene
            = FindObjectsByType<LSLStreamWriter>(FindObjectsSortMode.None);
            return streamWritersInScene.Any(
                writer => writer != this && writer.HasLiveOutlet && predicate(writer)
            );
        }


        /// <summary>
        /// Provides a source id string for the underlying outlet
        /// <br/> A combination of device id and application name by default
        /// </summary>
        protected virtual string BuildSourceID()
        {
            string deviceID = SystemInfo.deviceUniqueIdentifier;
            string applicationName = Application.productName;
            return $"{deviceID}-{applicationName}";
        }
    }
}