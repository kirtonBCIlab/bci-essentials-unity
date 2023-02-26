using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials.LSL
{
    public class LSLMarkerReceiver : MonoBehaviour, ILSLMarkerReceiver
    {
        public string Id { get; private set; }
        
        public StreamInfo StreamInfo { get; private set; }
        
        /// <summary>
        /// If this component has been initialized with a <see cref="StreamInlet"/>.
        /// </summary>
        public bool Initialized => _streamInlet != null;
        
        /// <summary>
        /// If the connection to the target stream is still open.
        /// </summary>
        public bool Connected => _streamInlet is { IsClosed: false };
        
        /// <summary>
        /// If the target stream is being polled for responses.
        /// </summary>
        public bool Polling => _polling != null;

        /// <summary>
        /// The duration in seconds between requests to the target stream for responses.
        /// </summary>
        public float PollingFrequency => _settings.PollingFrequency;

        /// <summary>
        /// The most recent response capture time.
        /// </summary>
        public double LastCaptureTime { get; private set; } = 0.0;
        
        /// <summary>
        /// The number of responses collected from the target stream.
        /// </summary>
        public int ResponsesCount => _allResponses.Count;

        /// <summary>
        /// The number of subscribers receiving marker updates.
        /// </summary>
        public int SubscriberCount => _subscribers.Count;

        
        public StreamInlet _streamInlet;
        private int _channelCount;
        private LSLMarkerReceiverSettings _settings = new();
        
        private Coroutine _polling;
        private readonly List<LSLMarkerResponse> _allResponses = new();
        private LSLMarkerResponse[] _lastPulledResponses = Array.Empty<LSLMarkerResponse>();

        private readonly List<ILSLMarkerSubscriber> _subscribers = new();

        public LSLMarkerReceiver Initialize(string id, StreamInfo streamInfo, LSLMarkerReceiverSettings settings = null)
        {
            Id = !string.IsNullOrEmpty(id) ? id : throw new ArgumentNullException(nameof(id));
            StreamInfo = streamInfo ?? throw new ArgumentNullException(nameof(streamInfo));
            _settings = settings ?? new LSLMarkerReceiverSettings();
            
            _streamInlet = new StreamInlet(streamInfo);
            _channelCount = StreamInfo.channel_count();

            Connect();
            
            return this;
        }

        public void CleanUp()
        {
            StopPolling();
            Disconnect();
        }
        
        private void OnDestroy()
        {
            CleanUp();
        }
        
        public void Subscribe(ILSLMarkerSubscriber subscriber)
        {
            _subscribers.Add(subscriber);

            if (!Polling)
            {
                StartPolling();
            }
        }

        public void Unsubscribe(ILSLMarkerSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);

            if (_subscribers.Count == 0)
            {
                StopPolling();
            }
        }

        /// <summary>
        /// Check for any new responses and returns all responses
        /// received since initialized.
        /// </summary>
        /// <returns>Array of <see cref="LSLMarkerResponse"/></returns>
        public LSLMarkerResponse[] GetResponses()
        {
            if (Connected)
            {
                PullSample();
            }

            return _allResponses.ToArray();

        }

        /// <summary>
        /// Check for any new responses and returns any responses
        /// retrieved.
        /// </summary>
        /// <returns>Array of <see cref="LSLMarkerResponse"/></returns>
        public LSLMarkerResponse[] GetLatestResponses()
        {
            if (Connected)
            {
                PullSample();
            }

            return _lastPulledResponses.ToArray();
        }
        
        private void Connect()
        {
            if (Connected)
            {
                Debug.LogWarning("Marker Receiver is already connected to a LSL Stream.");
                return;
            }
            
            _streamInlet?.open_stream();
        }

        private void Disconnect()
        {
            _streamInlet?.close_stream();
        }
        
        private void PullSample()
        {
            var pulledSamples = new List<LSLMarkerResponse>();
            var lastCaptureTime = double.MaxValue;
            var sample = new string[_channelCount];
            for(int i = 0 ; i < _channelCount ; i++) sample[i] = string.Empty;
            
            try
            {
                while (lastCaptureTime > 0)
                {
                    lastCaptureTime = _streamInlet.pull_sample(sample, 0);
                    if (lastCaptureTime == 0 || !HasValidValue(sample))
                    {
                        continue;
                    }

                    LastCaptureTime = lastCaptureTime;
                    
                    var sampleCopy = new string[sample.Length];
                    Array.Copy(sample, sampleCopy, sample.Length);
                    
                    var response = new LSLMarkerResponse(lastCaptureTime, sampleCopy);
                    
                    pulledSamples.Add(response);
                    _allResponses.Add(response);
                }
            }
            catch (LostException e)
            {
                Debug.LogError($"Lost connection to the StreamOutlet '{StreamInfo.name()}'");
            }

            if (pulledSamples.Count > 0)
            {
                _lastPulledResponses = pulledSamples.ToArray();
                NotifySubscribers(_lastPulledResponses);
            }
        }

        private void NotifySubscribers(LSLMarkerResponse[] responses)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.NewMarkersCallback(responses);
            }
        }

        private bool HasValidValue(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return true;
                }
            }

            return false;
        }

        private void StartPolling()
        {
            StopPolling();
            _polling = StartCoroutine(PollForSamples());
        }

        private void StopPolling()
        {
            if (_polling == null)
            {
                return;
            }

            StopCoroutine(_polling);
            _polling = null;
        }
        
        private IEnumerator PollForSamples()
        {
            Debug.Log($"Started polling stream for samples");
            int sampleCheck = 0;
            while (true)
            {
                ++sampleCheck;
                Debug.Log($"Pulling Sample {sampleCheck}; Tick: {Time.time}");

                PullSample();
                yield return new WaitForSecondsRealtime(PollingFrequency);
            }
        }
    }
}