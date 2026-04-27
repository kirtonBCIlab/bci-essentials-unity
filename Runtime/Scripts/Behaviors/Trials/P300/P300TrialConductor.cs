using UnityEngine;

namespace BCIEssentials
{
    using Stimulus.Collections;

    public abstract class P300TrialConductor : TrialConductor
    {
        public StimulusPresenterCollection PresenterCollection;
        public int FlashesPerOption;
        public float OnTime = 0.1f;
        public float OffTime = 0.075f;

        public P300TrialConductor(MonoBehaviour executionHost) : base(executionHost) { }
    }
}