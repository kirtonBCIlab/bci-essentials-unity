using System.Collections;
using UnityEngine;
using System;
using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    public class TVEPControllerBehavior : FrequencyStimulusControllerBehaviour
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.TVEP;

        [StartFoldoutGroup("Stimulus Frequency")]
        [SerializeField]
        [Tooltip("User-defined target stimulus frequency [Hz]")]
        private float requestedFlashingFrequency;
        [SerializeField, EndFoldoutGroup, InspectorReadOnly]
        [Tooltip("Calculated best-match achievable frequency based on the application framerate [Hz]")]
        private float realFlashingFrequency;


        protected override void SendEpochMarker(int trainingIndex = -1)
        => MarkerWriter.PushTVEPMarker(
            SPOCount, epochLength,
            new[] {realFlashingFrequency}, trainingIndex
        );


        protected override float GetRequestedFrequency(int index)
        => requestedFlashingFrequency;
        protected override void SetRealFrequency(int index, float value)
        => realFlashingFrequency = value;
    }
}