using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BCIEssentials
{
    using LSLFramework;
    using Utilities;

    public abstract class BCICommandCentre : MonoBehaviourUsingExtendedAttributes, ITargetIndicator, IPredictionSink
    {
        public abstract int TargetCount { get; }
        protected abstract TrialConductor TrialConductor { get; }

        [StartFoldoutGroup("Behaviour")]
        [SerializeField] protected AutomatedTrainingConductor _trainingConductor;

        [StartFoldoutGroup("Communication")]
        [SerializeField] protected MarkerWriter _markerWriter;
        [SerializeField] protected ResponseProvider _responseProvider;

        [StartFoldoutGroup("Keyboard Shortcuts")]
        [SerializeField] protected KeyBind _toggleTrialRunBinding;
        [SerializeField] protected KeyBind _toggleTrainingRunBinding;
        [SerializeField, EndFoldoutGroup] protected IndexedKeyBindSet _selectionBindings;


        protected virtual void Reset() => ResetKeyBinds();
        protected virtual void ResetKeyBinds()
        {
            _toggleTrialRunBinding = Key.S;
            _toggleTrainingRunBinding = Key.T;

            _selectionBindings = new IndexedKeyBindSet
            (
                (0, Key.Digit0), (1, Key.Digit1),
                (2, Key.Digit2), (3, Key.Digit3),
                (4, Key.Digit4), (5, Key.Digit5),
                (6, Key.Digit6), (7, Key.Digit7),
                (8, Key.Digit8), (9, Key.Digit9)
            );
        }


        protected virtual void Awake()
        {
            _trainingConductor.MarkerWriter ??= _markerWriter;
            TrialConductor.MarkerWriter ??= _markerWriter;
            _responseProvider.SubscribePredictions(OnPrediction);

            _trainingConductor.TrialConductor ??= TrialConductor;
            _trainingConductor.TargetIndicator ??= this;
        }

        protected virtual void Update() => ProcessKeyBinds();
        protected virtual void ProcessKeyBinds()
        {
            _toggleTrialRunBinding.CallIfPressedThisFrame(ToggleTrialRun);
            _toggleTrainingRunBinding.CallIfPressedThisFrame(ToggleTrainingRun);
            _selectionBindings.Process(MakeSelectionAtEndOfRun);
        }


        protected virtual void ToggleTrialRun() => ToggleConductorRun(TrialConductor);
        protected virtual void ToggleTrainingRun() => ToggleConductorRun(_trainingConductor);

        protected void ToggleConductorRun(CoroutineWrapper target)
        {
            if (!target.IsRunning) target.Begin(this);
            else target.Interrupt();
        }


        private void MakeSelectionAtEndOfRun(int selectionIndex)
        => StartCoroutine(RunTrialDelayedSelection(selectionIndex));

        private IEnumerator RunTrialDelayedSelection(int selectionIndex)
        {
            yield return TrialConductor.AwaitCompletion();
            OnPrediction(new MockPrediction(selectionIndex, TargetCount));
        }


        public abstract void OnPrediction(Prediction prediction);
        public abstract void BeginTargetIndication(int index);
        public abstract void EndTargetIndication();
    }
}
