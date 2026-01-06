using System.Collections;
using BCIEssentials.Behaviours.Trialing;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Selection
{
    /// <summary>
    /// Implements editable keyboard shortcuts to test BCI selection
    /// </summary>
    [RequireComponent(typeof(TrialBehaviour))]
    public class SelectorShortcuts : MonoBehaviourUsingExtendedAttributes
    {
        public IndexedKeyBindSet Bindings;

        [SerializeField] private ISelector _target;
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

            if (_trialBehaviour == null)
            {
                _trialBehaviour = GetComponent<TrialBehaviour>();
            }
            if (_target == null && TryGetComponent(out ISelector selector))
            {
                _target = selector;
            }
        }

        protected virtual void Update()
        => Bindings.Process(MakeSelectionAtEndOfRun);

        private void MakeSelectionAtEndOfRun(int selectionIndex)
        {
            if (_trialBehaviour)
            {
                StartCoroutine(RunTrialDelayedSelection(selectionIndex));
            }
            else Debug.Log("Missing reference to Trial Behaviour");
        }

        private IEnumerator RunTrialDelayedSelection(int selectionIndex)
        {
            yield return _trialBehaviour.AwaitCompletion();
            _target.MakeSelection(selectionIndex);
        }
    }
}