using UnityEngine;

namespace BCIEssentials.Behaviours
{
    using Extensions;
    using Utilities;

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
            this.CoalesceComponentReference(ref _target);
        }

        private void Start() => this.CoalesceComponentReference(ref _target);


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