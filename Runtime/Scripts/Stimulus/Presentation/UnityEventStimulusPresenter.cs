using UnityEngine.Events;

namespace BCIEssentials.Stimulus.Presentation
{
    public class UnityEventStimulusPresenter : StimulusPresentationBehaviour
    {
        public UnityEvent OnStimulusDisplayTrigged;
        public UnityEvent OnStimulusDisplayEnded;

        public UnityEvent OnSelected;

        public UnityEvent OnTargeted;
        public UnityEvent OnTargettingEnded;


        public override void StartStimulusDisplay()
        => OnStimulusDisplayTrigged?.Invoke();
        public override void EndStimulusDisplay()
        => OnStimulusDisplayEnded?.Invoke();

        public override void Select()
        => OnSelected?.Invoke();

        public override void StartTargetIndication()
        => OnTargeted?.Invoke();
        public override void EndTargetIndication()
        => OnTargettingEnded?.Invoke();
    }
}