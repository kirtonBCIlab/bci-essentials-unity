using System.Collections.Generic;
using BCIEssentials.Stimulus.Collections;
using BCIEssentials.Stimulus.Presentation.Standard;
using UnityEngine;

namespace BCIEssentials.Behaviours.Trialing
{
    public class SSVEPTrialBehaviour : PersistentTrialBehaviour
    {
        [Space]
        public int TargetFrameRate = 30;
        public List<FrequencyStimulusPresenter> Presenters;
        private float[] _calculatedFrequencies;


        protected virtual void Start()
        {
            Application.targetFrameRate = TargetFrameRate;
            RecalculateFrequencies(TargetFrameRate);
        }
        
        protected virtual void RecalculateFrequencies(int frameRate)
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
        (
            Presenters.Count, trainingIndex, EpochLength,
            _calculatedFrequencies
        );
        protected override void SendClassificationMarker()
        => MarkerWriter.PushSSVEPClassificationMarker
        (
            Presenters.Count, EpochLength,
            _calculatedFrequencies
        );
    }
}