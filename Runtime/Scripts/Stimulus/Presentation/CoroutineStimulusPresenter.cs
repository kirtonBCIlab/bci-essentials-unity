using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation
{
    public abstract class CoroutineStimulusPresenter : MonoBehaviour, IStimulusPresenter
    {
        public bool IsSelectable => enabled;

        private Coroutine _stimulusRoutine;


        public void StartStimulusDisplay()
        {
            _stimulusRoutine = StartCoroutine(RunStimulusDisplay());
        }
        public void EndStimulusDisplay()
        {
            StopCoroutine(_stimulusRoutine);
            StartCoroutine(RunStimulusCleanup());
        }

        protected abstract IEnumerator RunStimulusDisplay();
        protected abstract IEnumerator RunStimulusCleanup();


        public void Select()
        => StartCoroutine(RunSelection());
        protected abstract IEnumerator RunSelection();

        public void StartTargetIndication()
        => StartCoroutine(RunTargetIndication());
        protected abstract IEnumerator RunTargetIndication();
        public void EndTargetIndication()
        => StartCoroutine(RunTargetIndicationCleanup());
        protected abstract IEnumerator RunTargetIndicationCleanup();
    }
}