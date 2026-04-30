using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;

    public class P300CommandCentre : BCICommandCentre
    {
        public override int TargetCount => _targetIndicator.TargetCount;
        protected override TrialConductor TrialConductor => _trialConductor;

        [SerializeField, AppendToFoldoutGroup("Behaviour")]
        [ContextMenuItem("Locate Presenters", nameof(RepopulateStimulusPresenters))]
        protected P300TrialConductor _trialConductor;

        protected StimulusPresenterCollectionTargetIndicator _targetIndicator;


        protected override void Reset()
        {
            base.Reset();
            RepopulateStimulusPresenters();
        }


        [ContextMenu("Locate Presenters")]
        public void RepopulateStimulusPresenters()
        => _trialConductor.PresenterCollection.Repopulate(this);


        public override void OnPrediction(Prediction prediction)
        => _trialConductor.PresenterCollection.LatestSubset[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        => _targetIndicator.BeginTargetIndication(index);
        public override void EndTargetIndication()
        => _targetIndicator.EndTargetIndication();
    }
}