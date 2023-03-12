using System.Collections.Generic;
using System.Text;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public class LSLServiceProvider : MonoBehaviour, ILSLService
    {

        #region Inspector Properties

        [Header("LSL Marker Receivers")]
        [SerializeField]
        [Range(0, 30), Tooltip("The duration in seconds to reattempt to resolve the target stream.")]
        private double _resolveStreamTimeout;

        [SerializeField] private LSLMarkerReceiverSettings _responseStreamSettings = new();

        //See https://en.wikipedia.org/w/index.php?title=XPath_1.0&oldid=474981951#Node_set_functions for all options
        [SerializeField, Tooltip("Additional values to append to the resolve predicate. e.g. type='EEG' or 'count(info/desc/channel)=32'")]
        private string[] _additionalResolvePredicateValues;

        #endregion

        private static Dictionary<MultiValueKey, LSLMarkerReceiver> _markerReceivers =
            new(new MultiValueKeyHelpers.MultiValueKeyComparer());

        /// <summary>
        /// Register a <see cref="LSLMarkerReceiver"/> that wasn't created by
        /// a <see cref="LSLServiceProvider"/>.
        /// </summary>
        /// <param name="receiver">The <see cref="LSLMarkerReceiver"/> to register</param>
        /// <returns>Returns TRUE if the marker was registered.</returns>
        public bool RegisterMarkerReceiver(LSLMarkerReceiver receiver)
        {
            _markerReceivers ??= new Dictionary<MultiValueKey, LSLMarkerReceiver>();

            if (receiver == null)
            {
                return false;
            }

            var key = MultiValueKeyHelpers.GetKeyFromStreamInfo(receiver.StreamInfo);
            if (!SafeTryGetMarkerReceiver(key, MultiValueKeyHelpers.StrictKeyCompare, out _))
            {
                return _markerReceivers.TryAdd(key, receiver);
            }

            Debug.LogError($"The {nameof(LSLMarkerReceiver)} Id must have a value.");
            return false;
        }

        /// <summary>
        /// Retrieve an already created <see cref="LSLMarkerReceiver"/> from the
        /// service using its <see cref="MultiValueKey.UniqueKey"/>.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="MultiValueKey.UniqueKey"/> value to use.</param>
        /// <returns>The matching <see cref="LSLMarkerReceiver"/> or null</returns>
        public LSLMarkerReceiver GetMarkerReceiver(string uniqueKey)
        {
            SafeTryGetMarkerReceiver(new MultiValueKey(uniqueKey), MultiValueKeyCompareOptions.Hashcode, out var markerReceiver);
            return markerReceiver;
        }

        /// <summary>
        /// Retrieve an already created <see cref="LSLMarkerReceiver"/> from the
        /// service using its <see cref="MultiValueKey.UniqueKey"/>.
        /// </summary>
        /// <param name="key">The <see cref="MultiValueKey"/> value to use.</param>
        /// <returns>The matching <see cref="LSLMarkerReceiver"/> or null</returns>
        public LSLMarkerReceiver GetMarkerReceiver(MultiValueKey key)
        {
            SafeTryGetMarkerReceiver(key, MultiValueKeyCompareOptions.Hashcode, out var markerReceiver);
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

            MultiValueKey searchKey = null;
            switch (prop)
            {
                case "name":
                    searchKey = MultiValueKeyHelpers.GetKeyFromStreamProperties(name: propValue);
                    break;
                case "source_id":
                    searchKey = MultiValueKeyHelpers.GetKeyFromStreamProperties(sourceId: propValue);
                    break;
                case "type": 
                    searchKey = MultiValueKeyHelpers.GetKeyFromStreamProperties(type: propValue);
                    break;
                case "uid":
                default:
                    searchKey = new MultiValueKey(propValue);
                    break;
            }

            if (SafeTryGetMarkerReceiver(searchKey, MultiValueKeyHelpers.FlexibleKeyCompare, out receiver))
            {
                return true;
            }

            var streamInfo = LocateOpenStreamByProperty(prop, propValue);
            if (streamInfo == null)
            {
                return false;
            }

            var receiverKey = MultiValueKeyHelpers.GetKeyFromStreamInfo(streamInfo);
            receiver = new GameObject($"{nameof(LSLMarkerReceiver)}_{propValue}")
                .AddComponent<LSLMarkerReceiver>()
                .Initialize(receiverKey.UniqueKey, streamInfo, _responseStreamSettings);

            return _markerReceivers.TryAdd(receiverKey, receiver);
        }

        /// <summary>
        /// Gets a <see cref="LSLMarkerReceiver"/> from the collection.
        /// Removes entry from the collection if it is now null.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="compareAll">
        /// If true, will compare the all keys in the <paramref name="key"/>
        /// for against each other for a match.
        /// </param>
        /// <param name="markerReceiver">The <see cref="LSLMarkerReceiver"/> found.</param>
        /// <returns>TRUE if a <see cref="LSLMarkerReceiver"/> was found.</returns>
        private static bool SafeTryGetMarkerReceiver(MultiValueKey key, MultiValueKeyCompareOptions compareOptions,
            out LSLMarkerReceiver markerReceiver)
        {
            markerReceiver = null;

            if (key == null)
            {
                return false;
            }

            var deadReceivers = new List<MultiValueKey>();

            //FIND MATCH
            foreach (var (mvkey, receiver) in _markerReceivers)
            {
                if (receiver == null)
                {
                    deadReceivers.Add(mvkey);
                    continue;
                }

                if (MultiValueKeyHelpers.DoKeysMatch(mvkey, key, compareOptions))
                {
                    markerReceiver = receiver;
                    break;
                }
            }

            //DELETE DEAD
            foreach (var dKey in deadReceivers)
            {
                _markerReceivers.Remove(dKey);
            }

            return markerReceiver != null;
        }

        private StreamInfo LocateOpenStreamByProperty(string property, string value)
        {
            var predicateBuilder = new StringBuilder().Append($"{property}='{value}'");
            if (_additionalResolvePredicateValues != null)
            {
                foreach (var predValue in _additionalResolvePredicateValues)
                {
                    predicateBuilder.Append($" and ");
                    predicateBuilder.Append(predValue);
                }
            }

            var resolvePredicate = predicateBuilder.ToString();
            var streamInfos = LSL.LSL.resolve_stream(resolvePredicate, 0, _resolveStreamTimeout);
            return streamInfos.Length <= 0 ? null : streamInfos[0];
        }
    }
}