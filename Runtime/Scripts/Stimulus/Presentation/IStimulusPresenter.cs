namespace BCIEssentials.Stimulus.Presentation
{
    public interface IStimulusPresenter
    {
        public bool IsSelectable { get; }

        public void Select();

        public void StartTargetIndication();
        public void EndTargetIndication();

        public void TriggerStimulusDisplay();
        public void EndStimulusDisplay();
    }
}