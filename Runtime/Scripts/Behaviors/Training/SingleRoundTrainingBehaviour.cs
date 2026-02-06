using System.Collections;
using BCIEssentials.Behaviours.Trialing;
using UnityEngine;

namespace BCIEssentials.Behaviours.Training
{
    public class SingleRoundTrainingBehaviour : TrainingBehaviour, IBCIMarkerSource
    {
        [SerializeField]
        protected TargetIndicationBehaviour _targetIndicationBehaviour;
        [SerializeField]
        private TrialBehaviour _trialBehaviour;

        [StartFoldoutGroup("Marker Properties")]
        [SerializeField]
        [Tooltip("Whether to send 'Trial Started' and 'Trial Ends' markers. Disable if the training trial behaviour already sends these markers.")]
        private bool _sendTrialMarkers = true;

        [StartFoldoutGroup("Training Properties")]
        public int TargetIndex;

        public float TargetIndicationPeriod = 3.0f;
        public bool PersistTargetIndication = false;

        public float PreTrialTime = 0.5f;
        public float PostTrialTime = 0.0f;

        public bool SelectTrainingTarget = false;
        public float TargetSelectionDisplayPeriod = 0.5f;
        [EndFoldoutGroup]
        public float RestTime = 1.0f;



        protected override IEnumerator Run() => RunRound(TargetIndex);

        public virtual IEnumerator RunRound(int targetIndex)
        {
            if (_sendTrialMarkers && MarkerWriter != null) 
            {
                MarkerWriter.PushTrialStartedMarker();
            }
            
            _targetIndicationBehaviour.BeginTargetIndication(targetIndex);
            yield return new WaitForSeconds(TargetIndicationPeriod);

            if (!PersistTargetIndication)
            {
                _targetIndicationBehaviour.EndTargetIndication();
            }

            yield return new WaitForSeconds(PreTrialTime);
            _trialBehaviour.StartTrainingTrial(targetIndex);
            yield return _trialBehaviour.AwaitCompletion();
            yield return new WaitForSeconds(PostTrialTime);

            if (SelectTrainingTarget && _targetIndicationBehaviour != null)
            {
                _targetIndicationBehaviour.MakeSelection(targetIndex);
                yield return new WaitForSeconds(TargetSelectionDisplayPeriod);
            }

            if (PersistTargetIndication)
            {
                _targetIndicationBehaviour.EndTargetIndication();
            }

            if (_sendTrialMarkers && MarkerWriter != null) 
            {
                MarkerWriter.PushTrialEndsMarker();
            }
        }


        protected override void CleanUp()
        {
            if (_trialBehaviour.IsRunning) _trialBehaviour.Interrupt();
        }
    }
}