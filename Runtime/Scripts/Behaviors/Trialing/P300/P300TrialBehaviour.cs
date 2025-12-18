using BCIEssentials.Stimulus.Collections;

namespace BCIEssentials.Behaviours.Trialing.P300
{
    public abstract class P300TrialBehaviour : TrialBehaviour
    {
        public StimulusPresenterCollection PresenterCollection;
        public int FlashesPerOption = 10;
        public float OnTime = 0.1f;
        public float OffTime = 0.075f;
    }
}