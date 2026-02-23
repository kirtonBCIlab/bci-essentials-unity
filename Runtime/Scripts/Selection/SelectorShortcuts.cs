using System.Collections;
using BCIEssentials.Behaviours.Trialing;
using BCIEssentials.Extensions;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Selection
{
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
                (0, KeyCode.Alpha0), (1, KeyCode.Alpha1),
                (2, KeyCode.Alpha2), (3, KeyCode.Alpha3),
                (4, KeyCode.Alpha4), (5, KeyCode.Alpha5),
                (6, KeyCode.Alpha6), (7, KeyCode.Alpha7),
                (8, KeyCode.Alpha8), (9, KeyCode.Alpha9)
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