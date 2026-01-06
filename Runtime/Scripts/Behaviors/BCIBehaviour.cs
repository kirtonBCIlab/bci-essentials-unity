using System;
using BCIEssentials.Extensions;
using BCIEssentials.LSLFramework;
using UnityEngine;

namespace BCIEssentials.Behaviours
{
    using Trialing;
    using Training;
    using BCIEssentials.Selection;

    /// <summary>
    /// Behaviour skeleton for implementation of a BCI paradigm
    /// </summary>
    public class BCIBehaviour : MonoBehaviourUsingExtendedAttributes
    {
        public bool IsRunningTrial => _trialBehaviour.IsRunning;
        public bool IsRunningTraining => _trainingBehaviour.IsRunning;

        [SerializeField]
        private TrialBehaviour _trialBehaviour;
        [SerializeField]
        private TrainingBehaviour _trainingBehaviour;

        protected LSLMarkerWriter MarkerWriter;
        protected LSLResponseProvider ResponseProvider;


        /// <summary>
        /// Create or fetch reference to required LSL components,
        /// connecting any marker sources or selectors found on the same object
        /// </summary>
        protected void Initialize()
        {
            gameObject.GetOrAddComponent(ref MarkerWriter);
            gameObject.GetOrAddComponent(ref ResponseProvider);

            Array.ForEach(
                GetComponents<IBCIMarkerSource>(),
                source => source.MarkerWriter = MarkerWriter
            );
            Array.ForEach(
                GetComponents<ISelector>(),
                selector => ResponseProvider.SubscribePredictions(
                    prediction => selector.MakeSelection(prediction.Value)
                )
            );
        }


        public void StartTrial() => _trialBehaviour.Begin();
        public void InterruptTrial() => _trialBehaviour.Interrupt();

        public void StartTraining() => _trainingBehaviour.Begin();
        public void InterruptTraining() => _trainingBehaviour.Interrupt();

        public void UpdateClassifier() => MarkerWriter.PushUpdateClassifierMarker();
    }


    public interface IBCIMarkerSource
    {
        public LSLMarkerWriter MarkerWriter { set; }
    }
}