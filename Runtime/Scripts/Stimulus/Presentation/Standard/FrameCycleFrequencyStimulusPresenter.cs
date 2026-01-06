using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public class FrameCycleFrequencyStimulusPresenter : FrequencyStimulusPresenter
    {
        [Space]
        public int FramesOn = 1;
        public int FramesOff = 1;


        protected override IEnumerator RunDutyCycleDelay(bool on)
        {
            int frameDelay = on ? FramesOn : FramesOff;
            for (int i = 0; i < frameDelay; i++)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        public float CalculateNominalFrequency(float frameRate)
        {
            int framesPerDutyCycle = FramesOff + FramesOn;
            return frameRate / framesPerDutyCycle;
        }
    }
}