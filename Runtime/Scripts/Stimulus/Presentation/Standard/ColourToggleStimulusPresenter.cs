namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public class ColourToggleStimulusPresenter : ColourFlashBehaviour, IStimulusPresenter
    {
        public bool IsSelectable => enabled;

        public void TriggerStimulusDisplay()
        => SetRendererColour(OnColour);
        public void EndStimulusDisplay()
        => SetRendererColour(OffColour);

        public void Select() => StartSelectionIndication();
    }
}