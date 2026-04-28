using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;
    using Stimulus.Collections;

    public class SSVEPCommandCentre : BCICommandCentre
    {
        public override int TargetCount => _trialConductor.Presenters.WhereSelectable().Count;
        [SerializeField] new protected SSVEPTrialConductor _trialConductor;
        public int TargetFrameRate = 30;
        private int? _lastIndicatedTarget;


        protected virtual void Start()
        {
            Application.targetFrameRate = TargetFrameRate;
            _trialConductor.RecalculateMarkerFrequencies(TargetFrameRate);
        }


        public override void OnPrediction(Prediction prediction)
        => _trialConductor.Presenters[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        {
            _trialConductor.Presenters[index].StartTargetIndication();
            _lastIndicatedTarget = index;
        }
        public override void EndTargetIndication()
        {
            if (_lastIndicatedTarget.HasValue)
            {
                _trialConductor.Presenters[_lastIndicatedTarget.Value].EndTargetIndication();
            }
            _lastIndicatedTarget = null;
        }
    }
}