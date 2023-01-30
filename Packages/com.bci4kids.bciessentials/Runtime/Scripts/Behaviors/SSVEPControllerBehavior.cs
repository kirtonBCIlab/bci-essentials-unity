using System.Collections;
using UnityEngine;
using System;
using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    public class SSVEPControllerBehavior : BCIControllerBehavior
    {
        public override BCIBehaviorType BehaviorType => BCIBehaviorType.SSVEP;
        
        [SerializeField] private float[] setFreqFlash;
        [SerializeField] private float[] realFreqFlash;

        private int[] frames_on = new int[99];
        private int[] frame_count = new int[99];
        private float period;
        private int[] frame_off_count = new int[99];
        private int[] frame_on_count = new int[99];

        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);

            realFreqFlash = new float[_selectableSPOs.Count];

            var refreshRate = Application.targetFrameRate;
            for (int i = 0; i < _selectableSPOs.Count; i++)
            {

                frames_on[i] = 0;
                frame_count[i] = 0;
                period = refreshRate / setFreqFlash[i];
                // could add duty cycle selection here, but for now we will just get a duty cycle as close to 0.5 as possible
                frame_off_count[i] = (int)Math.Ceiling(period / 2);
                frame_on_count[i] = (int)Math.Floor(period / 2);
                realFreqFlash[i] = (refreshRate / (float)(frame_off_count[i] + frame_on_count[i]));

                Debug.Log($"frequency {i + 1} : {realFreqFlash[i]}");
            }
        }

        protected override IEnumerator SendMarkers(int trainingIndex = 99)
        {
            // Make the marker string, this will change based on the paradigm
            while (StimulusRunning)
            {
                // Desired format is: ["ssvep", number of options, training target (-1 if n/a), window length, frequencies]
                string freqString = "";
                for (int i = 0; i < realFreqFlash.Length; i++)
                {
                    freqString = freqString + "," + realFreqFlash[i].ToString();
                }

                string trainingString;
                if (trainingIndex <= _selectableSPOs.Count)
                {
                    trainingString = trainingIndex.ToString();
                }
                else
                {
                    trainingString = "-1";
                }

                string markerString = "ssvep," + _selectableSPOs.Count.ToString() + "," + trainingString + "," +
                                      windowLength.ToString() + freqString;

                // Send the marker
                marker.Write(markerString);

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
                        // turn the cube off
                        _selectableSPOs[i].StopStimulus();
                        frames_on[i] = 0;
                        frame_count[i] = 0;
                    }
                }
                else
                {
                    if (frame_count[i] >= frame_off_count[i])
                    {
                        // turn the cube on
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
