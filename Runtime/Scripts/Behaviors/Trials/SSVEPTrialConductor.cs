using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials
{
    using Stimulus.Collections;
    using Stimulus.Presentation.Standard;

    [System.Serializable]
    public class SSVEPTrialConductor : PersistentTrialConductor
    {
        [Space]
        public List<FrequencyStimulusPresenter> Presenters;
        private float[] _calculatedFrequencies;

        public SSVEPTrialConductor(MonoBehaviour executionHost) : base(executionHost) { }


        public virtual void RecalculateFrequencies(int frameRate)
        {
            int presenterCount = Presenters.Count;
            _calculatedFrequencies = new float[presenterCount];
            for (int i = 0; i < presenterCount; i++)
            {
                float frequency = Presenters[i] switch
                {
                    FrameCycleFrequencyStimulusPresenter spo => spo.CalculateNominalFrequency(frameRate),
                    TimeCycleFrequencyStimulusPresenter spo => spo.Frequency,
                    _ => 0
                };
                _calculatedFrequencies[i] = frequency;
            }
        }


        protected override void SetUp()
        {
            base.SetUp();
            Presenters.StartStimulusDisplay();
        }
        protected override void CleanUp()
        {
            base.CleanUp();
            Presenters.EndStimulusDisplay();
        }


        protected override void SendTrainingMarker(int trainingIndex)
        => MarkerWriter.PushSSVEPTrainingMarker
        (trainingIndex, EpochLength, _calculatedFrequencies);
        protected override void SendClassificationMarker()
        => MarkerWriter.PushSSVEPClassificationMarker
        (EpochLength, _calculatedFrequencies);
    }
}