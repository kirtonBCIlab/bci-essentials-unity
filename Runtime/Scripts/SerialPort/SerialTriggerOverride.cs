using UnityEngine;

namespace BCIEssentials.SerialPort
{
    /// <summary>
    /// Per-stimulus component that overrides the default serial trigger byte
    /// for a specific stimulus index.
    /// Registers with the parent <see cref="SerialMarkerWriter"/> on Start.
    /// </summary>
    public class SerialTriggerOverride : MonoBehaviour
    {
        [Tooltip("0-indexed stimulus index this override applies to")]
        public int StimulusIndex;

        [Tooltip("Trigger byte to send instead of the default (stimulusIndex + 1)")]
        public byte TriggerByte;

        private SerialMarkerWriter _writer;


        void Start()
        {
            _writer = GetComponentInParent<SerialMarkerWriter>();
            if (_writer == null)
            {
                Debug.LogWarning(
                    $"SerialTriggerOverride on '{gameObject.name}': "
                    + "no SerialMarkerWriter found in parent hierarchy."
                );
                return;
            }
            Register();
        }

        void OnDestroy()
        {
            Unregister();
        }


        public void Register()
        {
            if (_writer != null)
                _writer.RegisterStimulusOverride(StimulusIndex, TriggerByte);
        }

        public void Unregister()
        {
            if (_writer != null)
                _writer.UnregisterStimulusOverride(StimulusIndex);
        }
    }
}
