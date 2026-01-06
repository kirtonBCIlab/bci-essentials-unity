using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public abstract class FrequencyStimulusPresenter : ColourFlashBehaviour, IStimulusPresenter
    {
        public bool IsSelectable => enabled;
        public bool IsFlashing => _stimulusRoutine != null;
        Coroutine _stimulusRoutine;


        public virtual void Select()
        => StartSelectionIndication();


        public void StartStimulusDisplay()
        {
            SetUpStimulusDisplay();
            _stimulusRoutine = StartCoroutine(RunStimulus());
        }

        public void EndStimulusDisplay()
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
                SetRendererColour(OnColour);
                yield return RunDutyCycleDelay(true);
                SetRendererColour(OffColour);
                yield return RunDutyCycleDelay(false);
            }
        }

        protected abstract IEnumerator RunDutyCycleDelay(bool on);
        protected virtual void SetUpStimulusDisplay() { }
        protected virtual void CleanUpStimulusDisplay() { }
    }
}