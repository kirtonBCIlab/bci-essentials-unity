using UnityEngine;

namespace BCIEssentials.Stimulus
{
    public class ColourToggleStimulusPresenter : StimulusPresenter
    {
        [SerializeField] protected ColourFlashBehaviour _colourFlashBehaviour;

        public override void StartStimulusDisplay() => ToggleRendererColour(true);
        public override void EndStimulusDisplay() => ToggleRendererColour(false);

        public override void Select() => _colourFlashBehaviour.StartSelectionIndication(this);

        public override void StartTargetIndication() => _colourFlashBehaviour.StartTargetIndication(this);
        public override void EndTargetIndication() => _colourFlashBehaviour.EndTargetIndication();


        protected virtual void Awake() => _colourFlashBehaviour.SetUp();

        public void ToggleRendererColour(bool value)
        => _colourFlashBehaviour.SetColour(
            value ? _colourFlashBehaviour.OnColour
            : _colourFlashBehaviour.OffColour
        );
    }
}