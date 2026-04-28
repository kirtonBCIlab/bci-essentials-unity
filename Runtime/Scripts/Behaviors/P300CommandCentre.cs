using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;
    using Stimulus.Collections;

    public class P300CommandCentre : BCICommandCentre
    {
        public override int TargetCount => _stimulusPresenters.LatestSubset.Count;
        public enum FlashingPattern
        {
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
        => ReplaceTrialConductor(pattern switch
        {
            FlashingPattern.Random => new RandomFlashTrialConductor(this),
            FlashingPattern.ContextAware => new ContextAwareTrialConductor(this),
            FlashingPattern.RowColumn => new RowColumnFlashTrialConductor(this),
            FlashingPattern.Checkerboard => new CheckerboardFlashTrialConductor(this),
            FlashingPattern.ContextAwareGroups => new ContextAwareGroupsTrialConductor(this),
            _ => _trialConductor
        });

        public void ReplaceTrialConductor(P300TrialConductor newTrialConductor)
        {
            newTrialConductor.PresenterCollection = _stimulusPresenters;
            base.ReplaceTrialConductor(newTrialConductor);
        }


        public override void OnPrediction(Prediction prediction)
        => _stimulusPresenters.LatestSubset[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        => _targetIndicator.BeginTargetIndication(index);
        public override void EndTargetIndication()
        => _targetIndicator.EndTargetIndication();
    }
}