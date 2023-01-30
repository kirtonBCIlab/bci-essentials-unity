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
        private float _pullFrequency;
        
        /// <summary>
        /// The duration in seconds between requests to the target stream for responses.
        /// <para>Minimum value is 0.</para>
        /// </summary>
        public float PullFrequency
        {
            get => _pullFrequency;
            set
            {
                _pullFrequency = Mathf.Max(0, value);
            }
        }
        
        /// <summary>
        /// If the target stream was discovered and available.
        /// </summary>
        public bool Connected => _streamInlet is { IsClosed: false };
        
        /// <summary>
        /// If the target stream is being pulled for responses.
        /// </summary>
        public bool Pulling => _pulling != null;

        /// <summary>
        /// If responses have been received and stored during pulling.
        /// <see cref="GetResponses"/> does not reset this. 
        /// </summary>
        public bool HasStoredResponses => _responsesStore.Count > 0;
        
        private StreamInlet _streamInlet;
        private Coroutine _pulling;
        private readonly List<string> _responsesStore = new();

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
            
            _streamInlet = new StreamInlet(streamInfos[0]);
            _streamInlet.open_stream();
            
            Debug.Log($"Connected to stream: {_streamInlet}");
        }

        /// <summary>
        /// Dispose the open stream and clear responses.
        /// </summary>
        public void Disconnect()
        {
            if (Pulling)
            {
                StopPulling();
            }
            
            if (Connected)
            {
                _streamInlet?.close_stream();
                _streamInlet?.Dispose();
                _streamInlet = null;
            }
        }

        /// <summary>
        /// Begin pulling the target stream for responses.
        /// <para>
        /// Responses are stored until pulling stops, are requested or provided
        /// to the response callback.
        /// </para>
        /// </summary>
        /// <param name="onResponseCallback">An action to invoke when responses are received.</param>
        public void StartPulling(Action<string[]> onResponseCallback = null)
        {
            if (!Connected)
            {
                Debug.LogWarning($"The target stream is unavailable. Try calling the '{nameof(Connect)}' method.");
                return;
            }

            StopPulling();

            foreach (var response in GetResponses())
            {
                _responsesStore.Add(response);
            }
            
            _pulling = StartCoroutine(PullForSamples(onResponseCallback));
        }

        /// <summary>
        /// Stop pulling the target stream for responses.
        /// </summary>
        public void StopPulling()
        {
            _responsesStore.Clear();
            if (_pulling != null)
            {
                StopCoroutine(_pulling);
                _pulling = null;
            }
        }
        
        /// <summary>
        /// Retrieves all available responses either from the pulled
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
            
            if (HasStoredResponses)
            {
                foreach (var response in _responsesStore)
                {
                    responses.Add(response);
                }
                
                _responsesStore.Clear();
            }
            
            if (!Pulling)
            {
                var availableSamples = _streamInlet.samples_available();
                var sample = new[] { "" };

                for (int i = 0; i < availableSamples; i++)
                {
                    _streamInlet.pull_sample(sample);
                    responses.Add(sample[0]);
                }

                return responses.ToArray();
            }
            
            return responses.ToArray();
        }

        /// <summary>
        /// Clear the list of any responses received during pulling.
        /// </summary>
        public void ClearPulledResponses()
        {
            _responsesStore.Clear();
        }
        
        private IEnumerator PullForSamples(Action<string[]> onResponse = null)
        {
            Debug.Log($"Started pulling stream for samples");
            while (true)
            {
                var responses = new []{""};
                double result = _streamInlet.pull_sample(responses, 0);
                if (result != 0)
                {
                    foreach (var response in responses)
                    {
                        _responsesStore.Add(response);
                    }

                    if (onResponse != null)
                    {
                        onResponse.Invoke(responses);
                        _responsesStore.Clear();
                    }
                }
                
                yield return new WaitForSeconds(PullFrequency);
            }
        }
    }

    public interface IResponseStream
    {
        public bool Connected { get; }
        public bool HasStoredResponses { get; }
        
        public void Connect();
        public void Connect(string targetStringName);
        public void Disconnect();
        
        public void StartPulling(Action<string[]> onResponse = null);
        public void StopPulling();

        public string[] GetResponses();
        public void ClearPulledResponses();
    }
}