using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BCIEssentials.Selection
{
    using Behaviours.Trials;
    using Extensions;
    using Utilities;

    /// <summary>
    /// Implements editable keyboard shortcuts to test BCI selection
    /// </summary>
    [RequireComponent(typeof(SelectionBehaviour))]
    public class SelectorShortcuts : MonoBehaviourUsingExtendedAttributes
    {
        public IndexedKeyBindSet Bindings;

        [SerializeField] private SelectionBehaviour _target;
        [SerializeField] private TrialBehaviour _trialBehaviour;


        private void Reset()
        {
            Bindings = new IndexedKeyBindSet
            (
                (0, Key.Digit0), (1, Key.Digit1),
                (2, Key.Digit2), (3, Key.Digit3),
                (4, Key.Digit4), (5, Key.Digit5),
                (6, Key.Digit6), (7, Key.Digit7),
                (8, Key.Digit8), (9, Key.Digit9)
            );

            this.CoalesceComponentReference(ref _trialBehaviour);
            this.CoalesceComponentReference(ref _target);
        }

        private void Start()
        {
            this.CoalesceComponentReference(ref _trialBehaviour);
            this.CoalesceComponentReference(ref _target);
        }


        protected virtual void Update()
        => Bindings.Process(
            _trialBehaviour ? MakeSelectionAtEndOfRun
            : MakeSelection
        );

        private void MakeSelectionAtEndOfRun(int selectionIndex)
        => StartCoroutine(RunTrialDelayedSelection(selectionIndex));

        private IEnumerator RunTrialDelayedSelection(int selectionIndex)
        {
            yield return _trialBehaviour.AwaitCompletion();
            MakeSelection(selectionIndex);
        }

        private void MakeSelection(int selectionIndex)
        => _target.OnPrediction(new DummyPrediction(selectionIndex));
    }

    public class DummyPrediction : LSLFramework.Prediction
    {
        public DummyPrediction(int index) { Index = index; }
    }
}