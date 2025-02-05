using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    /// <summary>
    /// <para>
    /// A class that creates a <see cref="StreamInlet"/> and polls it for incoming samples.
    /// </para>
    /// <para>
    /// Allows subscribers to subscribe to polled samples without stealing samples from
    /// other subscribers.
    /// </para>
    /// </summary>
    public class LSLMarkerReceiver : MonoBehaviour, ILSLMarkerReceiver
    {
        public string UID { get; private set; }
        
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

        
        private StreamInlet _streamInlet;
        private int _channelCount;
        private LSLMarkerReceiverSettings _settings = new();
        
        private Coroutine _polling;
        private readonly List<LSLMarkerResponse> _allResponses = new();
        private LSLMarkerResponse[] _lastPulledResponses = Array.Empty<LSLMarkerResponse>();

        private readonly List<ILSLMarkerSubscriber> _subscribers = new();

        /// <summary>
        /// Initialize the Marker Receiver.
        /// </summary>
        /// <param name="streamInfo">The stream info to create a <see cref="StreamInlet"/> from.</param>
        /// <param name="settings">Optional settings to initialize with.</param>
        /// <returns>Initialized Marker Receiver</returns>
        /// <exception cref="ArgumentNullException">Throws if no <param name="streamInfo"></param> provided.</exception>
        /// <exception cref="TimeoutException">
        /// Throws if interacting with the <see cref="StreamInfo"/> or <see cref="StreamInlet"/>times out.
        /// </exception>
        public LSLMarkerReceiver Initialize(StreamInfo streamInfo, LSLMarkerReceiverSettings settings = null)
        {
            if (Initialized)
            {
                Debug.LogError("Cannot initialize an already initialized Marker Receiver.");
                return this;
            }

            if (streamInfo == null)
            {
                throw new ArgumentNullException(nameof(streamInfo));
            }
            
            _settings = settings ?? new LSLMarkerReceiverSettings();
            
            _streamInlet = new StreamInlet(streamInfo);
            StreamInfo = _streamInlet.info(_settings.GetInfoTimeout); //Throws if times out
            
            UID = StreamInfo.uid();
            _channelCount = StreamInfo.channel_count();

            try
            {
                _streamInlet.open_stream(_settings.OpenStreamTimeout);
            }
            catch (TimeoutException)
            {
                Debug.LogWarning("Failed to open stream within the timeout period. Stream will try open when polling starts.");
            }
            
            return this;
        }

        public void CleanUp()
        {
            StopPolling();
            
            UID = string.Empty;
            StreamInfo = null;
            
            _streamInlet?.Dispose();
            _streamInlet = null;
            
            _subscribers.Clear();
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
            PullSample();
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
        
        private void PullSample()
        {
            var pulledSamples = new List<LSLMarkerResponse>();
            var lastCaptureTime = double.MaxValue;
            var sample = new string[_channelCount];
            
            //Populate array with empty strings
            for(int i = 0 ; i < _channelCount ; i++) sample[i] = string.Empty;
            
            try
            {
                while (lastCaptureTime > 0)
                {
                    lastCaptureTime = _streamInlet.pull_sample(sample, _settings.PullSampleTimeout);
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
                Debug.LogError($"Lost connection to the StreamOutlet '{StreamInfo.name()}' + Error: {e.Message}");
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