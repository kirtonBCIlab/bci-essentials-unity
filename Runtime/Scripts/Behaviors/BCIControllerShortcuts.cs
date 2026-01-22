using BCIEssentials.Utilities;
using UnityEngine;

namespace BCIEssentials.Behaviours
{
    /// <summary>
    /// Implements editable keyboard shortcuts for BCI Controller methods.
    /// </summary>
    [RequireComponent(typeof(BCIController))]
    public class BCIControllerShortcuts: MonoBehaviourUsingExtendedAttributes
    {
        public KeyBind ToggleTrialRunBinding;
        public KeyBind ToggleTrainingRunBinding;
        public KeyBind UpdateClassifierBinding;

        [SerializeField, Space]
        private BCIController _target;


        private void Reset()
        {
            ToggleTrialRunBinding = KeyCode.S;
            ToggleTrainingRunBinding = KeyCode.T;
            UpdateClassifierBinding = KeyCode.Backspace;

            if (_target == null)
            {
                _target = GetComponent<BCIController>();
            }
        }

        protected virtual void Update()
        {
            ToggleTrialRunBinding.CallIfPressedThisFrame(ToggleTrialRun);
            ToggleTrainingRunBinding.CallIfPressedThisFrame(ToggleTrainingRun);
            UpdateClassifierBinding.CallIfPressedThisFrame(_target.UpdateClassifier);
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