using System.Collections;
using UnityEngine;
using System;
using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    public class TVEPControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.TVEP;

        [FoldoutGroup("Stimulus Frequency")]
        [SerializeField]
        [Tooltip("User-defined target stimulus frequency [Hz]")]
        private float requestedFlashingFrequency;
        [SerializeField, EndFoldoutGroup, InspectorReadOnly]
        [Tooltip("Calculated best-match achievable frequency based on the application framerate [Hz]")]
        private float realFlashingFrequency;

        private int[] frames_on = new int[99];
        private int[] frame_count = new int[99];
        private float period;
        private int[] frame_off_count = new int[99];
        private int[] frame_on_count = new int[99];

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frames_on[i] = 0;
                frame_count[i] = 0;
                period = targetFrameRate / requestedFlashingFrequency;
                frame_off_count[i] = (int)Math.Ceiling(period / 2);
                frame_on_count[i] = (int)Math.Floor(period / 2);
                realFlashingFrequency = (targetFrameRate / (float)(frame_off_count[i] + frame_on_count[i]));
            }
        }

        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (StimulusRunning)
            {
                // Send the marker
                OutStream.PushTVEPMarker(SPOCount, windowLength, new[] {realFlashingFrequency}, trainingIndex);

                // Wait the window length + the inter-window interval
                yield return new WaitForSecondsRealtime(windowLength + interWindowInterval);
            }
        }

        protected override IEnumerator OnStimulusRunBehavior()
        {
            // Add duty cycle
            // Generate the flashing
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frame_count[i]++;
                if (frames_on[i] == 1)
                {
                    if (frame_count[i] >= frame_on_count[i])
                    {
                        // turn the stimulus off
                        _selectableSPOs[i].StopStimulus();
                        frames_on[i] = 0;
                        frame_count[i] = 0;
                    }
                }
                else
                {
                    if (frame_count[i] >= frame_off_count[i])
                    {
                        // turn the stimulus on
                        _selectableSPOs[i].StartStimulus();
                        frames_on[i] = 1;
                        frame_count[i] = 0;
                    }
                }
            }
            yield return null;
        }

        protected override IEnumerator OnStimulusRunComplete()
        {
            foreach (var spo in _selectableSPOs)
            {
                if (spo != null)
                {
                    spo.StopStimulus();
                }
            }

            yield return null;
        }
    }
}