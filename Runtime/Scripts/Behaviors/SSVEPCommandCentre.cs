using System.Collections.Generic;
using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;
    using Stimulus.Collections;
    using Stimulus.Presentation.Standard;

    public class SSVEPCommandCentre : BCICommandCentre
    {
        public override int TargetCount => Presenters.WhereSelectable().Count;
        protected override TrialConductor TrialConductor => _trialConductor;
        protected List<FrequencyStimulusPresenter> Presenters => _trialConductor?.Presenters;

        [SerializeField, AppendToFoldoutGroup("Behaviour")]
        protected SSVEPTrialConductor _trialConductor;
        public int TargetFrameRate = 30;
        private int? _lastIndicatedTarget;


        protected virtual void Start()
        {
            Application.targetFrameRate = TargetFrameRate;
            _trialConductor.RecalculateMarkerFrequencies(TargetFrameRate);
        }


        public override void OnPrediction(Prediction prediction)
        => Presenters[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        {
            Presenters[index].StartTargetIndication();
            _lastIndicatedTarget = index;
        }
        public override void EndTargetIndication()
        {
            if (_lastIndicatedTarget.HasValue)
            {
                Presenters[_lastIndicatedTarget.Value].EndTargetIndication();
            }
            _lastIndicatedTarget = null;
        }
    }
}