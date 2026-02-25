using System.Collections.Generic;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    using static LSLStreamResolver;
    using static Response;

    public class LSLStreamReader : MonoBehaviour
    {
        [System.Flags]
        public enum ResponseTypes { None, Predictions, Pings }

        public string StreamType = "BCI_Essentials_Predictions";
        [SerializeField] bool _openOnStart = false;
        public ResponseTypes LoggingMask = ResponseTypes.None;
        protected bool LogPings => (LoggingMask & ResponseTypes.Pings) != 0;

        protected bool IsResolvingStream = false;

        public int SamplesAvailable => _inlet?.samples_available() ?? 0;
        public bool HasLiveInlet => _inlet is not null;
        private StreamInlet _inlet;
        private string[] _sampleBuffer;


        void Start()
        {
            if (_openOnStart) OpenStream();
        }

        void OnDestroy() => CloseStream();

        
        public void OpenStream(float resolutionPeriod = 0.1f)
        {
            IsResolvingStream = true;
            StartCoroutine(
                RunResolveByType(
                    StreamType, InitializeInlet,
                    resolutionPeriod
                )
            );
        }

        public virtual void CloseStream()
        {
            _inlet?.close_stream();
            _inlet?.Dispose();
            _inlet = null;
        }

        private void InitializeInlet(StreamInfo resolvedStreamInfo)
        {
            _sampleBuffer = new string[resolvedStreamInfo.channel_count()];
            _inlet = new(resolvedStreamInfo);
            IsResolvingStream = false;
            _inlet.open_stream(0.1);
        }

        
        public virtual Response[] PullAllResponses(int maxSamples = 50)
        {
            if (!HasLiveInlet)
            {
                Debug.LogWarning("The target stream is unavailable");
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
    }
}