using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public abstract class FrequencyStimulusPresenter : ColourFlashBehaviour, IStimulusPresenter
    {
        public bool IsSelectable => enabled;

        public Color OnColor = Color.white;
        public Color OffColor = Color.clear;

        Coroutine _stimulusRoutine;


        public void StartStimulusDisplay()
        => _stimulusRoutine = StartCoroutine(RunStimulus());
        public void EndStimulusDisplay()
        {
            if (_stimulusRoutine != null)
            {
                StopCoroutine(_stimulusRoutine);
            }
            else Debug.LogWarning("Can't end a stimulus routine that isn't running");
        }

        protected IEnumerator RunStimulus()
        {
            while (true)
            {
                SetRendererColour(OnColor);
                yield return RunDutyCycleDelay(true);
                SetRendererColour(OffColor);
                yield return RunDutyCycleDelay(false);
            }
        }

        protected abstract IEnumerator RunDutyCycleDelay(bool on);

        public virtual void Select()
        => StartSelectionIndication();
    }
}