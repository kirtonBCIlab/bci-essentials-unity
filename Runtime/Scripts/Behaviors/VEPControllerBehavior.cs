using System.Collections;
using System;
using BCIEssentials.Controllers;

namespace BCIEssentials.ControllerBehaviors
{
    public abstract class VEPControllerBehaviour : WindowedControllerBehavior
    {
        private int[] frames_on = new int[99];
        private int[] frame_count = new int[99];
        private float period;
        private int[] frame_off_count = new int[99];
        private int[] frame_on_count = new int[99];


        public override void PopulateObjectList(SpoPopulationMethod populationMethod = SpoPopulationMethod.Tag)
        {
            base.PopulateObjectList(populationMethod);
            InitializeFrequencies();

            for (int i = 0; i < _selectableSPOs.Count; i++)
            {
                frames_on[i] = 0;
                frame_count[i] = 0;
                period = targetFrameRate / GetRequestedFrequency(i);
                // could add duty cycle selection here, but for now we will just get a duty cycle as close to 0.5 as possible
                frame_off_count[i] = (int)Math.Ceiling(period / 2);
                frame_on_count[i] = (int)Math.Floor(period / 2);
                SetRealFrequency(i, targetFrameRate / (float)(frame_off_count[i] + frame_on_count[i]));
            }
        }

        protected virtual void InitializeFrequencies() {}
        protected abstract float GetRequestedFrequency(int index);
        protected abstract void SetRealFrequency(int index, float value);


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