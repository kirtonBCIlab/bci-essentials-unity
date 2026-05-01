using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BCIEssentials
{
    using Stimulus;

    [System.Serializable]
    public class SSVEPTrialConductor : PersistentTrialConductor
    {
        [Space]
        public List<FrequencyStimulusPresenter> Presenters;


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
        (trainingIndex, EpochLength, GetMarkerFrequencies());
        protected override void SendClassificationMarker()
        => MarkerWriter.PushSSVEPClassificationMarker
        (EpochLength, GetMarkerFrequencies());


        protected virtual float[] GetMarkerFrequencies()
        => Presenters.Select(p => p.Frequency).ToArray();
    }
}