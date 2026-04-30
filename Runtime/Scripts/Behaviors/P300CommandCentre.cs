using UnityEngine;

namespace BCIEssentials
{
    using Stimulus.Collections;
    using LSLFramework;

    public class P300CommandCentre : BCICommandCentre
    {
        public override int TargetCount => Presenters.LatestSubset.Count;
        protected override TrialConductor TrialConductor => _trialConductor;
        protected StimulusPresenterCollection Presenters => _trialConductor.PresenterCollection;

        [SerializeField, AppendToFoldoutGroup("Behaviour")]
        [ContextMenuItem("Locate Presenters", nameof(RepopulateStimulusPresenters))]
        protected P300TrialConductor _trialConductor;

        protected int? _lastIndicatedIndex;


        protected override void Reset()
        {
            base.Reset();
            RepopulateStimulusPresenters();
        }


        [ContextMenu("Locate Presenters")]
        public void RepopulateStimulusPresenters()
        {
            _trialConductor.PresenterCollection.Repopulate(this);
            Debug.Log($"Found {Presenters.Count} Stimulus Presenters Matching Search Criteria");
        }


        public override void OnPrediction(Prediction prediction)
        => _trialConductor.PresenterCollection.LatestSubset[prediction.Index].Select();

        public override void BeginTargetIndication(int index)
        {
            Presenters[index].StartTargetIndication();
            _lastIndicatedIndex = index;
        }
        public override void EndTargetIndication()
        {
            if (!_lastIndicatedIndex.HasValue)
            {
                Debug.LogWarning("No item has been targetted for training.");
                return;
            }
            Presenters[_lastIndicatedIndex.Value].EndTargetIndication();
            _lastIndicatedIndex = null;
        }
    }
}