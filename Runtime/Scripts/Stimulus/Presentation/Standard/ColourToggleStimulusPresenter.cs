using UnityEngine;

namespace BCIEssentials.Stimulus
{
    public class ColourToggleStimulusPresenter : StimulusPresenter
    {
        [SerializeField] protected ColourFlashBehaviour _colourFlashBehaviour;

        protected virtual void Awake() => _colourFlashBehaviour.SetUp();


        public override void StartStimulusDisplay() => _colourFlashBehaviour.DisplayOnColour();
        public override void EndStimulusDisplay() => _colourFlashBehaviour.DisplayOffColour();

        public override void Select() => _colourFlashBehaviour.StartSelectionIndication(this);

        public override void StartTargetIndication() => _colourFlashBehaviour.StartTargetIndication(this);
        public override void EndTargetIndication() => _colourFlashBehaviour.EndTargetIndication();
    }
}