using UnityEngine;
using UnityEngine.Events;

namespace BCIEssentials.Stimulus.Presentation
{
    public class UnityEventStimulusPresenter : MonoBehaviour, IStimulusPresenter
    {
        public bool IsSelectable => enabled;

        public UnityEvent OnStimulusDisplayTrigged;
        public UnityEvent OnStimulusDisplayEnded;

        public UnityEvent OnSelected;

        public UnityEvent OnTargeted;
        public UnityEvent OnTargettingEnded;


        public void StartStimulusDisplay()
        => OnStimulusDisplayTrigged?.Invoke();
        public void EndStimulusDisplay()
        => OnStimulusDisplayEnded?.Invoke();

        public void Select()
        => OnSelected?.Invoke();

        public void StartTargetIndication()
        => OnTargeted?.Invoke();
        public void EndTargetIndication()
        => OnTargettingEnded?.Invoke();
    }
}