using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace BCIEssentials.LSL
{
    public class LSLServiceProvider : MonoBehaviour, ILSLService
    {
        #region Internal Objects
        
        private class MultiValueKey
        {
            public readonly int HashCode;
            public readonly string UniqueKey;
            public readonly SortedSet<string> SecondaryKeys;

            public MultiValueKey(string primaryKey, string[] secondaryKeys)
            {
                if (primaryKey == null)
                {
                    throw new ArgumentNullException(nameof(primaryKey));
                }

                UniqueKey = primaryKey;
                HashCode = primaryKey.GetHashCode();

                if (secondaryKeys != null)
                {
                    SecondaryKeys = new();
                    foreach (var key in secondaryKeys)
                    {
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }
                        
                        SecondaryKeys.Add(key);
                    }
                }
            }

            public bool HasKey(string value)
            {
                if (UniqueKey == value)
                {
                    return true;
                }

                if (SecondaryKeys != null && SecondaryKeys.Contains(value))
                {
                    return true;
                }

                return false;
            }
        }
        
        #endregion
        
        #region Inspector Properties

        [Header("LSL Marker Receivers")]
        [SerializeField]
        [Range(0, 30), Tooltip("The duration in seconds to reattempt to resolve the target stream.")]
        private double _resolveStreamTimeout;

        [SerializeField] private LSLMarkerReceiverSettings _responseStreamSettings = new();

        #endregion

        private static Dictionary<MultiValueKey, LSLMarkerReceiver> _markerReceivers = new(new MarkerReceiverKeyComparer());

        /// <summary>
        /// Register a <see cref="LSLMarkerReceiver"/> that wasn't created by
        /// a <see cref="LSLServiceProvider"/>.
        /// </summary>
        /// <param name="receiver">The <see cref="LSLMarkerReceiver"/> to register</param>
        /// <returns>Returns TRUE if the marker was registered.</returns>
        public bool RegisterMarkerReceiver(LSLMarkerReceiver receiver)
        {
            _markerReceivers ??= new Dictionary<MultiValueKey, LSLMarkerReceiver>();
            
            if (!string.IsNullOrEmpty(receiver.Id) && !SafeTryGetMarkerReceiver(receiver.Id, out _))
            {
                var key = GetKeyForStreamInfo(receiver.StreamInfo);
                return _markerReceivers.TryAdd(key, receiver);
            }

            Debug.LogError($"The {nameof(LSLMarkerReceiver)} Id must have a value.");
            return false;
        }

        /// <summary>
        /// Retrieve an already created <see cref="LSLMarkerReceiver"/> from the
        /// service using its Id.
        /// </summary>
        /// <param name="markerReceiverId">The Id value</param>
        /// <returns>The matching <see cref="LSLMarkerReceiver"/> or null</returns>
        public LSLMarkerReceiver GetMarkerReceiver(string markerReceiverId)
        {
            SafeTryGetMarkerReceiver(markerReceiverId, out var markerReceiver);
            return markerReceiver;
        }
        
        /// <summary>
        /// Requests a <see cref="LSLMarkerReceiver"/> by Stream Name.
        /// Creates one if one does not exist but the stream was found.
        /// </summary>
        /// <param name="lslStreamName">The name of the LSL Stream to resolve.</param>
        /// <param name="markerReceiver">The <see cref="LSLMarkerReceiver"/> that is connected to the LSL stream.</param>
        /// <returns>TRUE if a LSL Stream was discovered.</returns>
        public bool TryGetMarkerReceiverByStreamName(string lslStreamName, out LSLMarkerReceiver markerReceiver)
        {
            return TryGetMarkerReceiverByProperty("name", lslStreamName, out markerReceiver);
        }

        /// <summary>
        /// Requests a <see cref="LSLMarkerReceiver"/> by Stream Id.
        /// Creates one if one does not exist but the stream was found.
        /// </summary>
        /// <param name="lslStreamId">The Id of the LSL Stream to resolve.</param>
        /// <param name="markerReceiver">The <see cref="LSLMarkerReceiver"/> that is connected to the LSL stream.</param>
        /// <returns>TRUE if a LSL Stream was discovered.</returns>
        public bool TryGetMarkerReceiverByStreamId(string lslStreamId, out LSLMarkerReceiver markerReceiver)
        {
            return TryGetMarkerReceiverByProperty("source_id", lslStreamId, out markerReceiver);
        }

        private bool TryGetMarkerReceiverByProperty(string prop, string propValue, out LSLMarkerReceiver receiver)
        {
            _markerReceivers ??= new Dictionary<MultiValueKey, LSLMarkerReceiver>();

            if (SafeTryGetMarkerReceiver(propValue, out receiver))
            {
                return true;
            }

            var streamInfo = LocateOpenStreamByProperty(prop, propValue);
            if (streamInfo == null)
            {
                return false;
            }
            
            var streamInlet = new StreamInlet(streamInfo);
            
            receiver = new GameObject($"{nameof(LSLMarkerReceiver)}_{propValue}")
                .AddComponent<LSLMarkerReceiver>()
                .Initialize(streamInfo.uid(), streamInlet, _responseStreamSettings);

            var key = GetKeyForStreamInfo(receiver.StreamInfo);
            return _markerReceivers.TryAdd(key, receiver);
        }

        /// <summary>
        /// Gets a <see cref="LSLMarkerReceiver"/> from the collection.
        /// Removes entry from the collection if it is now null.
        /// </summary>
        /// <param name="matchValue">Value to match against any of the key's.</param>
        /// <param name="markerReceiver">The <see cref="LSLMarkerReceiver"/> found.</param>
        /// <returns>TRUE if a <see cref="LSLMarkerReceiver"/> was found.</returns>
        private static bool SafeTryGetMarkerReceiver(string matchValue, out LSLMarkerReceiver markerReceiver)
        {
            var deadReceivers = new List<MultiValueKey>();
            markerReceiver = null;
            
            //FIND MATCH
            foreach (var (mvkey, receiver) in _markerReceivers)
            {
                if (receiver == null)
                {
                    deadReceivers.Add(mvkey);
                    continue;
                }

                if (mvkey.HasKey(matchValue))
                {
                    markerReceiver = receiver;
                    break;
                }
            }
            
            //DELETE DEAD
            foreach (var key in deadReceivers)
            {
                _markerReceivers.Remove(key);
            }

            return markerReceiver != null;
        }

        private static MultiValueKey GetKeyForStreamInfo(StreamInfo streamInfo)
        {
            return new MultiValueKey(streamInfo.uid(), new[]
            {
                streamInfo.name(),
                streamInfo.source_id(),
            });
        }

        private StreamInfo LocateOpenStreamByProperty(string property, string value)
        {
            var predicateBuilder = new StringBuilder().Append($"{property}='{value}'");
            if (_responseStreamSettings.AdditionalResolvePredicateValues != null)
            {
                foreach (var predValue in _responseStreamSettings.AdditionalResolvePredicateValues)
                {
                    predicateBuilder.Append($" and ");
                    predicateBuilder.Append(predValue);
                }
            }
            
            var resolvePredicate = predicateBuilder.ToString();
            var streamInfos = LSL.resolve_stream(resolvePredicate, 0, _resolveStreamTimeout);
            return streamInfos.Length <= 0 ? null : streamInfos[0];
        }
        
        private class MarkerReceiverKeyComparer : IEqualityComparer<MultiValueKey>
        {
            public bool Equals(MultiValueKey x, MultiValueKey y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.HashCode == y.HashCode;
            }

            public int GetHashCode(MultiValueKey obj)
            {
                return obj.HashCode;
            }
        }
    }
}