using System.Collections;
using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation
{
    public abstract class CoroutineStimulusPresenter : StimulusPresentationBehaviour
    {

        private Coroutine _stimulusRoutine;


        public override void StartStimulusDisplay()
        {
            _stimulusRoutine = StartCoroutine(RunStimulusDisplay());
        }
        public override void EndStimulusDisplay()
        {
            StopCoroutine(_stimulusRoutine);
            StartCoroutine(RunStimulusCleanup());
        }

        protected abstract IEnumerator RunStimulusDisplay();
        protected abstract IEnumerator RunStimulusCleanup();


        public override void Select()
        => StartCoroutine(RunSelection());
        protected abstract IEnumerator RunSelection();

        public override void StartTargetIndication()
        => StartCoroutine(RunTargetIndication());
        protected abstract IEnumerator RunTargetIndication();
        public override void EndTargetIndication()
        => StartCoroutine(RunTargetIndicationCleanup());
        protected abstract IEnumerator RunTargetIndicationCleanup();
    }
}