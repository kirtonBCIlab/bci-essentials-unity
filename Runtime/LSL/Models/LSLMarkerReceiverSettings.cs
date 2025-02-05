using System;
using UnityEngine;

namespace BCIEssentials.LSLFramework
{
    [Serializable]
    public class LSLMarkerReceiverSettings
    {
        [Min(0)]
        [Tooltip("The timout period when retrieving a streams info object. If the stream is invalid this could hang the application.")]
        public double GetInfoTimeout = 1D;
        
        [Min(0)]
        [Tooltip("The timout period when retrieving data from a stream. If the stream does not exist this could hang the application.")]
        public double OpenStreamTimeout = 1D;
        
        [Min(0)]
        [Tooltip("The timout period when retrieving data from a stream. If the stream does not exist this could hang the application.")]
        public double PullSampleTimeout = 1D;
        
        [Min(0), Tooltip("The duration in seconds between requests to the target stream for responses. Value of 0 polls once every update.")]
        public float PollingFrequency = 0;
    }
}