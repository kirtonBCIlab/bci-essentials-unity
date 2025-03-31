using UnityEngine;
using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    public class SSVEPControllerBehavior : VEPControllerBehaviour
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.SSVEP;
        
        [StartFoldoutGroup("Stimulus Frequencies")]
        [SerializeField]
        [Tooltip("User-defined set of target stimulus frequencies [Hz]")]
        private float[] requestedFlashingFrequencies;
        [SerializeField, EndFoldoutGroup, InspectorReadOnly]
        [Tooltip("Calculated best-match achievable frequencies based on the application framerate [Hz]")]
        private float[] realFlashingFrequencies;


        protected override void SendWindowMarker(int trainingIndex = -1)
        => MarkerWriter.PushSSVEPMarker(
            SPOCount, windowLength,
            realFlashingFrequencies, trainingIndex
        );


        protected override void InitializeFrequencies()
        => realFlashingFrequencies = new float[_selectableSPOs.Count];

        protected override float GetRequestedFrequency(int index)
        => requestedFlashingFrequencies[index];
        protected override void SetRealFrequency(int index, float value)
        => requestedFlashingFrequencies[index] = value;
    }
}
