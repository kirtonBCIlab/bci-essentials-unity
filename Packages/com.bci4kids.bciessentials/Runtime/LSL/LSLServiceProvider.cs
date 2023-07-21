using System;
using System.Collections.Generic;
using LSL;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    public class LSLServiceProvider : MonoBehaviour, ILSLService 
    {
        private const string k_StreamIdPredicateFormat = "uid='{0}'";
        private const string k_StreamNamePredicateFormat = "name='{0}'";
        
        #region Inspector Properties
        
        [Header("LSL Marker Receivers")]
        [SerializeField]
        [Range(0, 30), Tooltip("The duration in seconds to reattempt to resolve the target stream.")]
        private double _resolveStreamTimeout;

        [SerializeField] private LSLMarkerReceiverSettings _responseStreamSettings = new();
        #endregion

        private static readonly Dictionary<string, ILSLMarkerReceiver> _markerReceivers = new();
        #region Marker Receivers

        /// <summary>
        /// Register a <see cref="LSLMarkerReceiver"/> that wasn't created by
        /// a <see cref="LSLServiceProvider"/>.
        /// </summary>
        /// <param name="receiver">The <see cref="LSLMarkerReceiver"/> to register</param>
        /// <returns>Returns TRUE if the marker was registered.</returns>
        public bool RegisterMarkerReceiver(ILSLMarkerReceiver receiver)
        {
            if (receiver == null)
            {
                return false;
            }
            
            if (SafeTryGetMarkerReceiver(receiver.UID, out _))
            {
                Debug.LogError($"A {nameof(LSLMarkerReceiver)} is already registered for Id: {receiver.UID}.");
                return false;
            }

            _markerReceivers[receiver.UID] = receiver;
            return true;
        }

        /// <summary>
        /// Check if a Marker Receiver is currently registered with this Service Provider.
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>TRUE if the receiver is registered</returns>
        public bool HasRegisteredMarkerReceiver(ILSLMarkerReceiver receiver)
        {
            return _markerReceivers.ContainsValue(receiver);
        }
        
        /// <summary>
        /// Retrieve a Marker Receiver using its <see cref="StreamInfo.uid()"/>.
        /// <para>
        /// If the service provider does not have a receiver for <param name="uid"></param>
        /// it will try locate an open stream and create a marker receiver if a
        /// stream is found.
        /// </para>
        /// </summary>
        /// <param name="uid">UID for the given stream.</param>
        /// <returns>NULL if no receiver found.</returns>
        public ILSLMarkerReceiver GetMarkerReceiverByUID(string uid)
        {
            //Try find existing registered
            if (SafeTryGetMarkerReceiver(uid, out var receiver))
            {
                return receiver;
            }
            
            //Resolve using LSL predicate searching
            var streamInfo = ResolveStream(string.Format(k_StreamIdPredicateFormat, uid));
            if (streamInfo == null)
            {
                return null;
            }

            //Create, Register, Return
            receiver = NewMarkerReceiver(streamInfo, _responseStreamSettings);
            _markerReceivers.TryAdd(uid, receiver);
            return receiver;
        }

        /// <summary>
        /// Retrieve a Marker Receiver using its <see cref="StreamInfo.name()"/>.
        /// <para>
        /// Will try locate using a predicate first and if an open stream is found
        /// then the service provider will check if has a registered Marker Receiver
        /// and otherwise register it as one.
        /// </para>
        /// </summary>
        /// <param name="streamName">Name of the stream.</param>
        /// <returns>NULL if no receiver found.</returns>
        public ILSLMarkerReceiver GetMarkerReceiverByName(string streamName)
        {
            return GetMarkerReceiverByPredicate(string.Format(k_StreamNamePredicateFormat, streamName));
        }

        /// <summary>
        /// Retrieve a Marker Receiver using a predicate.
        /// <para>
        /// See <a href="https://en.wikipedia.org/wiki/XPath">XPath 1.0</a> for predicate formatting.
        /// </para>
        /// <para>
        /// If the service provider does not have a receiver for <param name="uid"></param>
        /// it will try locate an open stream and create a marker receiver if a
        /// stream is found.
        /// </para>
        /// </summary>
        /// <param name="predicate">The predicate value to search by.</param>
        /// <example>"source_id='id'"</example>
        /// <returns>NULL if no receiver found.</returns>
        public ILSLMarkerReceiver GetMarkerReceiverByPredicate(string predicate)
        {
            //Just use LSL predicate searching
            var streamInfo = ResolveStream(predicate);
            if (streamInfo == null)
            {
                return null;
            }

            //Try find existing registered
            var streamId = streamInfo.uid();
            if (SafeTryGetMarkerReceiver(streamId, out var receiver))
            {
                return receiver;
            }

            //Create, Register, Return
            receiver = NewMarkerReceiver(streamInfo, _responseStreamSettings);
            _markerReceivers.TryAdd(streamId, receiver);

            return receiver;
        }

        /// <summary>
        /// Unregister a Marker Receiver from Service Provider.
        /// </summary>
        /// <param name="receiver">The Marker Receiver to unregister</param>
        public void UnregisterMarkerReceiver(ILSLMarkerReceiver receiver)
        {
            if (_markerReceivers.ContainsValue(receiver))
            {
                _markerReceivers.Remove(receiver.UID);
                receiver.CleanUp();
            }
        }
        
        /// <summary>
        /// Gets a <see cref="LSLMarkerReceiver"/> from the collection.
        /// Removes entry from the collection if it is now null.
        /// </summary>
        /// <param name="id">The key to use.</param>
        /// <param name="markerReceiver">The <see cref="LSLMarkerReceiver"/> found.</param>
        /// <returns>TRUE if a <see cref="LSLMarkerReceiver"/> was found.</returns>
        private static bool SafeTryGetMarkerReceiver(string id, out ILSLMarkerReceiver markerReceiver)
        {
            if (!_markerReceivers.TryGetValue(id, out markerReceiver))
            {
                return false;
            }

            if (markerReceiver is not MonoBehaviour monoReceiver || monoReceiver != null)
            {
                return true;
            }
            
            _markerReceivers.Remove(id);
            return false;

        }
        
        private ILSLMarkerReceiver NewMarkerReceiver(StreamInfo streamInfo, LSLMarkerReceiverSettings settings = null)
        {
            return new GameObject($"{nameof(LSLMarkerReceiver)}_{streamInfo.uid()}")
                .AddComponent<LSLMarkerReceiver>()
                .Initialize(streamInfo, settings);
        }
        #endregion
        
        private StreamInfo ResolveStream(string predicate)
        {
            var streams = LSL.LSL.resolve_stream(predicate, 0, _resolveStreamTimeout);
            var firstStream = streams.Length <= 0 ? null : streams[0];
            if (firstStream == null)
            {
                streams.DisposeArray();
                return null;
            }
            
            return firstStream;
        }
    }
}