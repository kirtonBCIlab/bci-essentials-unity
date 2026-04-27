using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;
    using Stimulus.Collections;

    public class P300CommandCentre : BCICommandCentre
    {
        public override int TargetCount => _stimulusPresenters.Count;
        public enum FlashingPattern {
            Random, ContextAware,
            RowColumn, Checkerboard, ContextAwareGroups
        }

        [SerializeField] protected StimulusPresenterCollection _stimulusPresenters;
        protected StimulusPresenterCollectionTargetIndicator _targetIndicator;


        protected override void Reset()
        {
            base.Reset();
            ReplaceTrialConductor(FlashingPattern.Random);
        }

        public void ReplaceTrialConductor(FlashingPattern pattern)
        {
            _trialConductor = pattern switch
            {
                FlashingPattern.Random => new SingleFlashTrialConductor(this),
                FlashingPattern.ContextAware => new ContextAwareSingleFlashTrialConductor(this),
                FlashingPattern.RowColumn => new RowColumnFlashTrialConductor(this),
                FlashingPattern.Checkerboard => new CheckerboardFlashTrialConductor(this),
                FlashingPattern.ContextAwareGroups => new ContextAwareMultiFlashTrialConductor(this),
                _ => null
            };
            _trialConductor.MarkerWriter = _markerWriter;
            _trainingConductor.TrialConductor = _trialConductor;
        }


        public override void OnPrediction(Prediction prediction)
        => _stimulusPresenters.LatestSubset[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        => _targetIndicator.BeginTargetIndication(index);
        public override void EndTargetIndication()
        => _targetIndicator.EndTargetIndication();
    }
}