using System.Collections.Generic;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    using static LSLStreamResolver;
    using static LSLResponse;

    public class LSLStreamReader: MonoBehaviour
    {
        public string StreamType = "BCI";
        [SerializeField] bool _openOnStart = false;
        public bool PrintLogs = false;

        protected bool IsResolvingStream = false;

        public int SamplesAvailable => _inlet?.samples_available() ?? 0;
        protected bool HasLiveInlet => _inlet is not null;
        private StreamInlet _inlet;
        private string[] _sampleBuffer;


        void Start()
        {
            if (_openOnStart) OpenStream();
        }

        void OnDestroy() => CloseStream();

        
        public void OpenStream()
        {
            IsResolvingStream = true;
            StartCoroutine(RunResolveByType(StreamType, InitializeInlet));
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
            _inlet.open_stream(0);
        }

        
        public virtual LSLResponse[] PullAllResponses(int maxSamples = 50)
        {
            if (!HasLiveInlet)
            {
                Debug.LogWarning("The target stream is unavailable");
                return new LSLResponse[0];
            }

            List<LSLResponse> pulledResponses = new();
            double lastCaptureTime = double.MaxValue;
            int pullCounter = 0;

            while (lastCaptureTime > 0 && pullCounter++ < maxSamples)
            {
                lastCaptureTime = PullResponse(out LSLResponse response);
                if (lastCaptureTime > 0)
                    pulledResponses.Add(response);
            }
            return pulledResponses.ToArray();
        }

        private double PullResponse(out LSLResponse parsedResponse)
        {
            double captureTime = _inlet.pull_sample(_sampleBuffer, 0);
            parsedResponse = BuildResponse(_sampleBuffer, captureTime);
            if (PrintLogs)
            {
                Debug.Log($"Pulled {parsedResponse}");
            }
            return captureTime;
        }
    }
}