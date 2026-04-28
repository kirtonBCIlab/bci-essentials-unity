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

        [SerializeField] protected TrialConductor _trialConductor;
        [SerializeField] protected AutomatedTrainingConductor _trainingConductor;

        [StartFoldoutGroup("Communication")]
        [SerializeField] protected MarkerWriter _markerWriter;
        [SerializeField] protected ResponseProvider _responseProvider;

        [StartFoldoutGroup("Keyboard Shortcuts")]
        [SerializeField] protected KeyBind _toggleTrialRunBinding;
        [SerializeField] protected KeyBind _toggleTrainingRunBinding;
        [SerializeField, EndFoldoutGroup] protected IndexedKeyBindSet _selectionBindings;


        protected virtual void Reset()
        {
            _markerWriter = new();
            _responseProvider = new();
            _trainingConductor = new(this, this);
            ResetKeyBinds();
        }

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


        public void ReplaceTrialConductor(TrialConductor newTrialConductor)
        {
            _trialConductor = newTrialConductor;
            _trialConductor.MarkerWriter = _markerWriter;
            _trainingConductor.TrialConductor = _trialConductor;
        }


        protected virtual void Update() => ProcessKeyBinds();
        protected virtual void ProcessKeyBinds()
        {
            _toggleTrialRunBinding.CallIfPressedThisFrame(ToggleTrialRun);
            _toggleTrainingRunBinding.CallIfPressedThisFrame(ToggleTrainingRun);
            _selectionBindings.Process(MakeSelectionAtEndOfRun);
        }


        protected virtual void ToggleTrialRun() => ToggleConductorRun(_trialConductor);
        protected virtual void ToggleTrainingRun() => ToggleConductorRun(_trainingConductor);

        protected void ToggleConductorRun(CoroutineWrapper target)
        {
            if (!target.IsRunning) target.Begin();
            else target.Interrupt();
        }


        private void MakeSelectionAtEndOfRun(int selectionIndex)
        => StartCoroutine(RunTrialDelayedSelection(selectionIndex));

        private IEnumerator RunTrialDelayedSelection(int selectionIndex)
        {
            yield return _trialConductor.AwaitCompletion();
            OnPrediction(new DummyPrediction(selectionIndex, TargetCount));
        }


        public abstract void OnPrediction(Prediction prediction);
        public abstract void BeginTargetIndication(int index);
        public abstract void EndTargetIndication();


        public class DummyPrediction : Prediction
        {
            public DummyPrediction(int index) { Index = index; }
            public DummyPrediction(int selectedIndex, int targetCount) : this(selectedIndex)
            {
                Probabilities = new float[targetCount];
                float selectionProbability = Random.Range(1f / targetCount, 1f);
                float remainingProbability = 1f - selectionProbability;

                for (int i = 0; i < targetCount; i++)
                {
                    if (i == selectedIndex) Probabilities[i] = selectionProbability;
                    else
                    {
                        float maximumProbability = Mathf.Min(selectionProbability, remainingProbability);
                        Probabilities[i] = Random.Range(0, maximumProbability);
                        remainingProbability -= Probabilities[i];
                    }
                }
            }
        }
    }
}
