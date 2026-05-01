using UnityEngine;

namespace BCIEssentials.Stimulus.Presentation.Standard
{
    public class ColourToggleStimulusPresenter : StimulusPresenter
    {
        [SerializeField] protected ColourFlashBehaviour _colourFlashBehaviour;

        public override void StartStimulusDisplay() => ToggleRendererColour(true);
        public override void EndStimulusDisplay() => ToggleRendererColour(false);

        public override void Select() => _colourFlashBehaviour.StartSelectionIndication();

        public override void StartTargetIndication() => _colourFlashBehaviour.StartTargetIndication();
        public override void EndTargetIndication() => _colourFlashBehaviour.EndTargetIndication();


        public void ToggleRendererColour(bool value)
        => _colourFlashBehaviour.SetColour(
            value ? _colourFlashBehaviour.OnColour
            : _colourFlashBehaviour.OffColour
        );
    }
}