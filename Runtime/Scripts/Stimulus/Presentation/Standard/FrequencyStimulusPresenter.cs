using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus
{
    public abstract class FrequencyStimulusPresenter : ColourToggleStimulusPresenter
    {
        public bool IsFlashing => _stimulusRoutine != null;
        Coroutine _stimulusRoutine;


        public override void StartStimulusDisplay()
        {
            SetUpStimulusDisplay();
            _stimulusRoutine = StartCoroutine(RunStimulus());
        }

        public override void EndStimulusDisplay()
        {
            if (_stimulusRoutine != null)
            {
                StopCoroutine(_stimulusRoutine);
            }
            else Debug.LogWarning("Can't end a stimulus routine that isn't running");

            CleanUpStimulusDisplay();
        }

        protected IEnumerator RunStimulus()
        {
            while (true)
            {
                ToggleRendererColour(true);
                yield return RunDutyCycleDelay(true);
                ToggleRendererColour(false);
                yield return RunDutyCycleDelay(false);
            }
        }

        protected abstract IEnumerator RunDutyCycleDelay(bool on);
        protected virtual void SetUpStimulusDisplay() { }
        protected virtual void CleanUpStimulusDisplay() { }
    }
}