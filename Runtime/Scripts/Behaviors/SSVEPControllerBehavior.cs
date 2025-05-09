using UnityEngine;
using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    public class SSVEPControllerBehavior : FrequencyStimulusControllerBehaviour
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.SSVEP;
        
        [StartFoldoutGroup("Stimulus Frequencies")]
        [SerializeField]
        [Tooltip("User-defined set of target stimulus frequencies [Hz]")]
        private float[] requestedFlashingFrequencies;
        [SerializeField, EndFoldoutGroup, InspectorReadOnly]
        [Tooltip("Calculated best-match achievable frequencies based on the application framerate [Hz]")]
        private float[] realFlashingFrequencies;


        protected override void SendTrainingMarker(int trainingIndex)
        => MarkerWriter.PushSSVEPTrainingMarker(
            SPOCount, trainingIndex, epochLength, realFlashingFrequencies
        );

        protected override void SendClassificationMarker()
        => MarkerWriter.PushSSVEPClassificationMarker(
            SPOCount, epochLength, realFlashingFrequencies
        );


        protected override void UpdateObjectListConfiguration()
        {
            realFlashingFrequencies = new float[SPOCount];
            base.UpdateObjectListConfiguration();
        }

        protected override float GetRequestedFrequency(int index)
        => requestedFlashingFrequencies[index];
        protected override void SetRealFrequency(int index, float value)
        => realFlashingFrequencies[index] = value;
    }
}
