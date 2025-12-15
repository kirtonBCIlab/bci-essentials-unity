namespace BCIEssentials.Selection.Stimulus
{
    public interface IStimulusPresenter : ISelectable
    {
        public void TriggerStimulusDisplay();
        public void EndStimulusDisplay();
    }
}