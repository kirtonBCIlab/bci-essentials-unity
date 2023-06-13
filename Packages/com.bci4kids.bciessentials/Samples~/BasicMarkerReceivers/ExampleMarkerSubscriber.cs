using System.Linq;
using BCIEssentials.LSLFramework;
using BCIEssentials.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace BCIEssentials.LSL.Samples
{
    public class ExampleMarkerSubscriber : MonoBehaviour, ILSLMarkerSubscriber
    {
        [Header("Configuration")] [SerializeField]
        private LSLServiceProvider _provider;

        [SerializeField] private string _streamName = "MyStream";
        [SerializeField] private bool _subscribeOnStart = true;

        [Header("UI")] [SerializeField] private Button _subscribeButton;
        [SerializeField] private Button _unsubscribeButton;
        [SerializeField] private Text _responseText;

        [Space(20), InspectorReadOnly] public LSLMarkerReceiver LslMarkerReceiver;

        public bool Subscribed { get; private set; }

        private void Start()
        {
            _responseText.text = "No Responses";
            _subscribeButton.onClick.AddListener(Subscribe);
            _unsubscribeButton.onClick.AddListener(Unsubscribe);

            if (_provider == null)
            {
                return;
            }

            LslMarkerReceiver = _provider.GetMarkerReceiverByName(_streamName);
            if (LslMarkerReceiver != null && _subscribeOnStart)
            {
                Subscribe();
            }
        }

        public void Subscribe()
        {
            if (!Subscribed && LslMarkerReceiver != null)
            {
                Debug.Log("Subscribed!");
                LslMarkerReceiver.Subscribe(this);
                Subscribed = true;
            }
        }

        public void Unsubscribe()
        {
            if (Subscribed && LslMarkerReceiver != null)
            {
                Debug.Log("Unsubscribed!");
                LslMarkerReceiver.Unsubscribe(this);
                Subscribed = false;
            }
        }

        public void NewMarkersCallback(LSLMarkerResponse[] latestMarkers)
        {
            Debug.Log($"{latestMarkers.Length} New Markers Received");

            var responseStrings = latestMarkers.Select(r => r.Value[0]).ToArray();
            _responseText.text = $"Responses:\n{string.Join(", ", responseStrings)}";
        }
    }
}