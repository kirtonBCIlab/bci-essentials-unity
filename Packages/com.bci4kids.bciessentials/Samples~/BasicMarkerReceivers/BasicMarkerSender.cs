using System.Collections;
using LSL;
using UnityEngine;
using UnityEngine.UI;

namespace BCIEssentials.LSL.Samples
{
    public class BasicMarkerSender: MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _streamName = "MyStream";
        [SerializeField] private string _markerValue = "MyMarker";
        [SerializeField, Min(1)] private float _sendFrequencySeconds = 1;

        [Header("UI")]
        [SerializeField] private Text _responseText;
        
        private StreamOutlet _outlet;
        
        private void Awake()
        {
            var streamInfo = new StreamInfo(_streamName, "EEG", 1, 0.0, channel_format_t.cf_string, gameObject.GetInstanceID().ToString());
            _outlet = new StreamOutlet(streamInfo);

            StartCoroutine(ContinuousPushMarker());
        }

        private IEnumerator ContinuousPushMarker()
        {
            int markerCount = 1;
            while (true)
            {
                var markerValue = $"{_markerValue} ({markerCount})";
                ++markerCount;
                
                _responseText.text = $"Latest Marker: {markerValue}";
                _outlet.push_sample(new []{markerValue});

                yield return new WaitForSecondsRealtime(_sendFrequencySeconds);
            }
        }
    }
}