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

        //See https://en.wikipedia.org/w/index.php?title=XPath_1.0&oldid=474981951#Node_set_functions for all options
        [Tooltip("Additional values to append to the resolve predicate. e.g. type='EEG' or 'count(info/desc/channel)=32'")]
        public string[] AdditionalResolvePredicateValues;
    }
}