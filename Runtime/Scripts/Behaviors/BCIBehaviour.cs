using System;
using BCIEssentials.LSLFramework;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Behaviours
{
    /// <summary>
    /// Base class for any implementation of a BCI paradigm
    /// </summary>
    public abstract class BCIBehaviour : MonoBehaviourUsingExtendedAttributes
    {
        public bool IsRunningTrial => _trialBehaviour.IsRunning;
        public bool IsRunningTraining => _trainingBehaviour.IsRunning;

        [SerializeField]
        private CoroutineBehaviour _trialBehaviour;
        [SerializeField]
        private CoroutineBehaviour _trainingBehaviour;

        protected LSLMarkerWriter MarkerWriter;
        protected LSLResponseProvider ResponseProvider;


        protected void Initialize()
        {
            gameObject.GetOrAddComponent(ref MarkerWriter);
            gameObject.GetOrAddComponent(ref ResponseProvider);
            ResponseProvider.SubscribePredictions(prediction => MakeSelection(prediction.Value));

            Array.ForEach(
                GetComponents<IBCIMarkerSource>(),
                source => source.MarkerWriter = MarkerWriter
            );
            Array.ForEach(
                GetComponents<IBCIResponseListener>(),
                listener => listener.ResponseProvider = ResponseProvider
            );
        }


        public void StartTrial() => _trialBehaviour.Begin();
        public void InterruptTrial() => _trialBehaviour.Interrupt();

        public void StartTraining() => _trainingBehaviour.Begin();
        public void InterruptTraining() => _trainingBehaviour.Interrupt();


        protected abstract void MakeSelection(int index);
    }


    public interface IBCIMarkerSource
    {
        public LSLMarkerWriter MarkerWriter { set; }
    }

    public interface IBCIResponseListener
    {
        public LSLResponseProvider ResponseProvider { set; }
    }
}