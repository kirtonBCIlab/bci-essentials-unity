namespace BCIEssentials.Behaviours.Trials.P300
{
    using Stimulus.Collections;

    public abstract class P300TrialBehaviour : TrialBehaviour
    {
        public StimulusPresenterCollection PresenterCollection;
        public int FlashesPerOption = 10;
        public float OnTime = 0.1f;
        public float OffTime = 0.075f;
    }
}