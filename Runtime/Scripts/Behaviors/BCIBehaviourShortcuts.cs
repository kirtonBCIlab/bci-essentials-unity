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
        public KeyBind StartTrialBinding;
        public KeyBind StartTrainingBinding;
        public KeyBind UpdateClassifierBinding;

        [SerializeField, Space]
        private BCIBehaviour _target;


        private void Reset()
        {
            StartTrialBinding = KeyCode.S;
            StartTrainingBinding = KeyCode.T;
            UpdateClassifierBinding = KeyCode.Backspace;

            if (_target == null) _target = GetComponent<BCIBehaviour>();
        }

        protected virtual void Update()
        => ProcessShortcuts(
            new (KeyBind, Action)[] {
                (StartTrialBinding, _target.StartTrial),
                (StartTrainingBinding, _target.StartTraining),
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
    }
}