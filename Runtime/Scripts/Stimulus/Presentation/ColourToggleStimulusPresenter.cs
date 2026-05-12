using UnityEngine;

namespace BCIEssentials.Stimulus
{
    public class ColourToggleStimulusPresenter : StimulusPresenter
    {
        [SerializeField] protected ColourFlashDisplay _colourFlashDisplay;

        protected virtual void Awake() => _colourFlashDisplay.SetUp();


        public override void StartStimulusDisplay() => _colourFlashDisplay.DisplayOnColour();
        public override void EndStimulusDisplay() => _colourFlashDisplay.DisplayOffColour();

        public override void Select() => _colourFlashDisplay.StartSelectionIndication(this);

        public override void StartTargetIndication() => _colourFlashDisplay.StartTargetIndication(this);
        public override void EndTargetIndication() => _colourFlashDisplay.EndTargetIndication();
    }
}