using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace BCIEssentials.LSL
{
    public class LSLResponseStream : MonoBehaviour, IResponseStream
    {
        [SerializeField]
        [Tooltip("The name of the open stream to pull responses from.")]
        private string _targetStreamName = "PythonResponse";
        
        [SerializeField]
        [Tooltip("The duration in seconds to reattempt to resolve the target stream.")]
        [UnityEngine.Range(1, 30)]
        private double _resolveTimeout;
        
        [SerializeField]
        [Tooltip("The duration in seconds between requests to the target stream for responses.")]
        [Min(0)]
        private float _pollFrequency;
        
        /// <summary>
        /// The duration in seconds between requests to the target stream for responses.
        /// <para>Minimum value is 0.</para>
        /// </summary>
        public float PollFrequency
        {
            get => _pollFrequency;
            set
            {
                _pollFrequency = Mathf.Max(0, value);
            }
        }
        
        /// <summary>
        /// If the target stream was discovered and available.
        /// </summary>
        public bool Connected => _responseInlet is { IsClosed: false };
        
        /// <summary>
        /// If the target stream is being polled for responses.
        /// </summary>
        public bool Polling => _receivingMarkers != null;

        /// <summary>
        /// If responses have been received and stored during polling.
        /// <see cref="GetResponses"/> does not reset this. 
        /// </summary>
        public bool HasPolledResponses => _responses.Count > 0;
        
        private StreamInlet _responseInlet;
        private Coroutine _receivingMarkers;
        private readonly List<string> _responses = new();

        /// <summary>
        /// Attempt to resolve the target stream using <see cref="_targetStreamName"/>.
        /// </summary>
        /// <returns>Returns true if the target stream was resolved.</returns>
        public void Connect()
        {
            Connect(_targetStreamName);
        }
        
        /// <summary>
        /// Attempt to resolve the target stream using provided stream name.
        /// </summary>
        /// <param name="targetStreamName">The name of the stream to resolve.</param>
        /// <returns>Returns true if the target stream was resolved.</returns>
        public void Connect(string targetStreamName)
        {
            Disconnect();

            var streamInfos = LSL.resolve_stream("name", targetStreamName, 0, _resolveTimeout);
            if (streamInfos.Length <= 0) return;
            
            _responseInlet = new StreamInlet(streamInfos[0]);
            _responseInlet.open_stream();
            
            Debug.Log($"Connected to stream: {_responseInlet}");
        }

        /// <summary>
        /// Dispose the open stream and clear responses.
        /// </summary>
        public void Disconnect()
        {
            if (Polling)
            {
                StopPolling();
            }
            
            if (Connected)
            {
                _responseInlet?.close_stream();
                _responseInlet?.Dispose();
                _responseInlet = null;
            }
        }

        /// <summary>
        /// Begin polling the target stream for responses.
        /// <para>
        /// Responses are stored until polling stops, are requested or provided
        /// to the response callback.
        /// </para>
        /// </summary>
        /// <param name="onResponseCallback">An action to invoke when responses are received.</param>
        public void StartPolling(Action<string[]> onResponseCallback = null)
        {
            if (!Connected)
            {
                Debug.LogWarning($"The target stream is unavailable. Try calling the '{nameof(Connect)}' method.");
                return;
            }

            StopPolling();

            foreach (var response in GetResponses())
            {
                _responses.Add(response);
            }
            
            _receivingMarkers = StartCoroutine(PollForSamples(onResponseCallback));
        }

        /// <summary>
        /// Stop polling the target stream for responses.
        /// </summary>
        public void StopPolling()
        {
            _responses.Clear();
            if (_receivingMarkers != null)
            {
                StopCoroutine(_receivingMarkers);
                _receivingMarkers = null;
            }
        }
        
        /// <summary>
        /// Retrieves all available responses either from the polled
        /// collection or from the stream directly.
        /// </summary>
        /// <returns>Returns an array of stream responses.</returns>
        public string[] GetResponses()
        {
            if (!Connected)
            {
                Debug.LogWarning($"The target stream is unavailable. Try calling the '{nameof(Connect)}' method.");
                return Array.Empty<string>();
            }

            var responses = new List<string>();
            
            if (HasPolledResponses)
            {
                foreach (var response in _responses)
                {
                    responses.Add(response);
                }
                
                _responses.Clear();
            }
            
            if (!Polling)
            {
                var availableSamples = _responseInlet.samples_available();
                var sample = new[] { "" };

                for (int i = 0; i < availableSamples; i++)
                {
                    _responseInlet.pull_sample(sample);
                    responses.Add(sample[0]);
                }

                return responses.ToArray();
            }
            
            return responses.ToArray();
        }

        /// <summary>
        /// Clear the list of any responses received during polling.
        /// </summary>
        public void ClearPolledResponses()
        {
            _responses.Clear();
        }
        
        private IEnumerator PollForSamples(Action<string[]> onResponse = null)
        {
            Debug.Log($"Started polling stream for samples");
            while (true)
            {
                var responses = new []{""};
                double result = _responseInlet.pull_sample(responses, 0);
                if (result != 0)
                {
                    foreach (var response in responses)
                    {
                        _responses.Add(response);
                    }

                    if (onResponse != null)
                    {
                        onResponse.Invoke(responses);
                        _responses.Clear();
                    }
                }
                
                yield return new WaitForSeconds(PollFrequency);
            }
        }
    }

    public interface IResponseStream
    {
        public bool Connected { get; }
        public bool HasPolledResponses { get; }
        
        public void Connect();
        public void Connect(string targetStringName);
        public void Disconnect();
        
        public void StartPolling(Action<string[]> onResponse = null);
        public void StopPolling();

        public string[] GetResponses();
        public void ClearPolledResponses();
    }
}