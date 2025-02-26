using System.Collections.Generic;
using UnityEngine;
using LSL;
using BCIEssentials.LSLFramework;

namespace BCIEssentials.Tests.Utilities
{
    using static LSLStreamResolver;

    public class LSLRawStreamReader: MonoBehaviour
    {
        private StreamInlet _inlet;
        private string[] _sampleBuffer;

        public void OpenStreamByName(string name)
        {
            if (TryResolveByName(name, out StreamInfo streamInfo))
                InitializeInlet(streamInfo);
            else
                Debug.LogWarning("Failed to Resolve Stream");
        }

        private void InitializeInlet(StreamInfo resolvedStreamInfo)
        {
            _sampleBuffer = new string[resolvedStreamInfo.channel_count()];
            _inlet = new(resolvedStreamInfo);
            _inlet.open_stream(0.1);
        }


        public virtual string[] PullAllSamples(int maxSamples = 50)
        {
            if (_inlet is null)
            {
                Debug.LogWarning("The target stream is unavailable");
                return new string[0];
            }

            List<string> pulledSamples = new();
            double lastCaptureTime = double.MaxValue;
            int pullCounter = 0;

            while (lastCaptureTime > 0 && pullCounter++ < maxSamples)
            {
                lastCaptureTime = _inlet.pull_sample(_sampleBuffer, 0);
                if (lastCaptureTime > 0)
                    pulledSamples.Add(_sampleBuffer[0]);
            }
            return pulledSamples.ToArray();
        }
    }
}