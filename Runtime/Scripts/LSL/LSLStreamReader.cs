using System.Collections.Generic;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    using System.Linq;
    using static LSLStreamResolver;
    using static Response;

    [System.Serializable]
    public class LSLStreamReader
    {
        [System.Flags]
        public enum ResponseTypes { None, Predictions, Pings }
        private static readonly List<LSLStreamReader> _connectedReaders = new();

        public string StreamType = "BCI_Essentials_Predictions";
        public ResponseTypes LoggingMask = ResponseTypes.None;
        protected bool LogPings => (LoggingMask & ResponseTypes.Pings) != 0;

        protected bool IsResolvingStream = false;

        public bool TargetStreamExists => TryResolveByType(StreamType, out _);
        public bool IsConnected => (_inlet is not null) && TargetStreamExists;
        public int SamplesAvailable => _inlet?.samples_available() ?? 0;
        private StreamInlet _inlet;
        private string[] _sampleBuffer;


        public void FindAndOpenStream(float resolutionPeriod = 0.1f)
        {
            IsResolvingStream = true;
            StartTypeResolutionThread(
                StreamType, InitializeInlet,
                resolutionPeriod
            );
        }

        ~LSLStreamReader() => CloseStream();
        public virtual void CloseStream()
        {
            _inlet?.close_stream();
            _inlet?.Dispose();
            _inlet = null;

            _connectedReaders.Remove(this);
        }

        private void InitializeInlet(StreamInfo resolvedStreamInfo)
        {
            WarnIfTypeInUse();

            _sampleBuffer = new string[resolvedStreamInfo.channel_count()];
            _inlet = new(resolvedStreamInfo);
            IsResolvingStream = false;
            _inlet.open_stream(0.1);

            _connectedReaders.Add(this);
        }


        public virtual Response[] PullAllResponses(int maxSamples = 50)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Inlet isn't connected to stream");
                return new Response[0];
            }

            List<Response> pulledResponses = new();
            double lastCaptureTime = double.MaxValue;
            int pullCounter = 0;

            while (lastCaptureTime > 0 && pullCounter++ < maxSamples)
            {
                lastCaptureTime = PullResponse(out Response response);
                if (lastCaptureTime > 0 && response is not EmptyResponse)
                    pulledResponses.Add(response);
            }
            return pulledResponses.ToArray();
        }

        private double PullResponse(out Response parsedResponse)
        {
            double captureTime = _inlet.pull_sample(_sampleBuffer, 0);
            parsedResponse = BuildResponse(_sampleBuffer, captureTime);
            if (
                LoggingMask != ResponseTypes.None
                && parsedResponse is not EmptyResponse
                && (parsedResponse is not Ping || LogPings)
            )
            {
                Debug.Log($"Pulled {parsedResponse}");
            }
            return captureTime;
        }


        private bool ConnectedReaderSharesType(LSLStreamReader other)
        => other.IsConnected && other.StreamType == StreamType;

        private void WarnIfTypeInUse()
        {
            if (_connectedReaders.Any(ConnectedReaderSharesType))
                Debug.LogWarning(
                    "Another Stream Reader is already connected to a "
                    + $"stream of the type {StreamType}."
                );
        }
    }
}