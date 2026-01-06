using System;
using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Behaviours
{
    /// <summary>
    /// Implements editable keyboard shortcuts for BCI Behaviour methods.
    /// </summary>
    [RequireComponent(typeof(BCIBehaviour))]
    public class BCIBehaviourShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyBind ToggleTrialRunBinding;
        public KeyBind ToggleTrainingRunBinding;
        public KeyBind UpdateClassifierBinding;

        [SerializeField, Space]
        private BCIBehaviour _target;


        private void Reset()
        {
            ToggleTrialRunBinding = KeyCode.S;
            ToggleTrainingRunBinding = KeyCode.T;
            UpdateClassifierBinding = KeyCode.Backspace;

            if (_target == null) _target = GetComponent<BCIBehaviour>();
        }

        protected virtual void Update()
        => ProcessShortcuts(
            new (KeyBind, Action)[] {
                (ToggleTrialRunBinding, ToggleTrialRun),
                (ToggleTrainingRunBinding, ToggleTrainingRun),
                (UpdateClassifierBinding, _target.UpdateClassifier)
            }
        );

        private void ProcessShortcuts(
            (KeyBind, Action)[] shortcuts
        )
        {
            foreach ((KeyBind keyBind, Action method) in shortcuts)
                if (keyBind.WasPressedThisFrame) method();
        }


        private void ToggleTrialRun()
        {
            if (!_target.IsRunningTrial) _target.StartTrial();
            else _target.InterruptTrial();
        }

        private void ToggleTrainingRun()
        {
            if (!_target.IsRunningTraining) _target.StartTraining();
            else _target.InterruptTraining();
        }
    }
}