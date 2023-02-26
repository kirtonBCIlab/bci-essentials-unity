using System;
using UnityEngine;

namespace BCIEssentials.LSL
{
    [Serializable]
    public class LSLMarkerReceiverSettings
    {
        [Min(-1)]
        [Tooltip("The timout period when retrieving data from a stream. If the stream does not exist this could hang the application.")]
        public double StreamTimeout = 10D;
        
        [Min(0), Tooltip("The duration in seconds between requests to the target stream for responses. Value of 0 polls once every update.")]
        public float PollingFrequency = 0;
    }
}