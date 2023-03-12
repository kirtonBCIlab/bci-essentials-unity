using System.Linq;
using BCIEssentials.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BCIEssentials.LSL.Samples
{
    public class ExampleManualMarkerReceiver : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LSLServiceProvider _provider;
        [SerializeField] private string _streamName = "MyStream";
        [SerializeField] private bool _getOnlyLatest;
        
        [Header("UI")]
        [SerializeField] private Button _requestButton;
        [SerializeField] private Text _responseText;

        [Space(20), InspectorReadOnly]
        public LSLMarkerReceiver LslMarkerReceiver;

        private void Start()
        {
            _requestButton.onClick.AddListener(GetResponses);
            _provider.TryGetMarkerReceiverByStreamName(_streamName, out LslMarkerReceiver);
        }

        public void GetResponses()
        {
            var responses = _getOnlyLatest ? LslMarkerReceiver.GetLatestResponses() : LslMarkerReceiver.GetResponses();
            
            var responseStrings = responses.Select(r => r.Value[0]).ToArray();
            _responseText.text = $"Responses:\n{string.Join(",\n", responseStrings)}";
        }
    }
}