using System.Collections;
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
        public bool IsRunningSequence => _sequenceRoutine != null;
        public bool IsRunningTrial { get; protected set; }
        public bool IsRunningTraining { get; protected set; }

        // training behaviour

        protected LSLMarkerWriter MarkerWriter;
        protected LSLResponseProvider ResponseProvider;

        private Coroutine _sequenceRoutine;


        protected void Initialize()
        {
            gameObject.FindOrCreateComponent(ref MarkerWriter);
            gameObject.FindOrCreateComponent(ref ResponseProvider);
            ResponseProvider.SubscribePredictions(prediction => MakeSelection(prediction.Value));
        }


        public abstract void StartTraining();

        protected abstract void SetUpSequence();
        protected abstract void CleanUpSequence();
        protected abstract IEnumerator RunSequence();

        protected abstract void MakeSelection(int index);


        public void StartSequence()
        {
            if (IsRunningSequence)
            {
                Debug.LogWarning("Attempted to start sequence, but it is already running.");
                return;
            }

            SetUpSequence();
            _sequenceRoutine = StartCoroutine(RunSequence());
            CleanUpSequence();
        }

        public void StopSequence()
        {
            if (!IsRunningSequence)
            {
                Debug.LogWarning("Can't stop a sequence that isn't running.");
                return;
            }

            StopCoroutine(_sequenceRoutine);
            CleanUpSequence();
        }


        protected IEnumerator AwaitSequenceCompletion()
        {
            while (IsRunningSequence) yield return null;
        }
    }
}