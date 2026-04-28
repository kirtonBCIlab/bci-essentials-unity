using UnityEngine;

namespace BCIEssentials
{
    using LSLFramework;
    using Stimulus.Collections;

    public class P300CommandCentre : BCICommandCentre
    {
        public override int TargetCount => _presenterCollection.LatestSubset.Count;
        public enum FlashingPattern
        {
            Random, ContextAware,
            RowColumn, Checkerboard, ContextAwareGroups
        }

        [ContextMenuItem("Locate Presenters", nameof(RepopulateStimulusPresenters))]
        [SerializeField] protected DynamicStimulusPresenterCollection _presenterCollection;
        protected StimulusPresenterCollectionTargetIndicator _targetIndicator;


        protected override void Reset()
        {
            base.Reset();
            ReplaceTrialConductor(FlashingPattern.Random);
        }

        public void ReplaceTrialConductor(FlashingPattern pattern)
        => ReplaceTrialConductor(pattern switch
        {
            FlashingPattern.Random => new RandomFlashTrialConductor(),
            FlashingPattern.ContextAware => new ContextAwareTrialConductor(),
            FlashingPattern.RowColumn => new RowColumnFlashTrialConductor(),
            FlashingPattern.Checkerboard => new CheckerboardFlashTrialConductor(),
            FlashingPattern.ContextAwareGroups => new ContextAwareGroupsTrialConductor(),
            _ => _trialConductor
        });

        public void ReplaceTrialConductor(TrialConductor newTrialConductor)
        {
            if (newTrialConductor is P300TrialConductor p300TrialConductor)
            {
                p300TrialConductor.PresenterCollection = _presenterCollection;
            }
            _trialConductor = newTrialConductor;
        }


        [ContextMenu("Locate Presenters")]
        public void RepopulateStimulusPresenters() => _presenterCollection.Repopulate(this);


        public override void OnPrediction(Prediction prediction)
        => _presenterCollection.LatestSubset[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        => _targetIndicator.BeginTargetIndication(index);
        public override void EndTargetIndication()
        => _targetIndicator.EndTargetIndication();
    }
}